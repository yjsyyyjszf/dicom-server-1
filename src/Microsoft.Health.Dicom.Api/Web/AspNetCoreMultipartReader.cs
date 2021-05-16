﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.Health.Abstractions.Exceptions;
using Microsoft.Health.Dicom.Core.Configs;
using Microsoft.Health.Dicom.Core.Exceptions;
using Microsoft.Health.Dicom.Core.Web;
using Microsoft.Net.Http.Headers;
using NotSupportedException = Microsoft.Health.Dicom.Core.Exceptions.NotSupportedException;

namespace Microsoft.Health.Dicom.Api.Web
{
    /// <summary>
    /// Multipart reader implemented by using AspNetCore's <see cref="MultipartReader"/>.
    /// </summary>
    internal class AspNetCoreMultipartReader : IMultipartReader
    {
        private const string TypeParameterName = "type";
        private const string StartParameterName = "start";
        private readonly ISeekableStreamConverter _seekableStreamConverter;
        private readonly IOptions<StoreConfiguration> _storeConfiguration;
        private readonly string _rootContentType;
        private readonly MultipartReader _multipartReader;

        private int _sectionIndex;

        internal AspNetCoreMultipartReader(
            string contentType,
            Stream body,
            ISeekableStreamConverter seekableStreamConverter,
            IOptions<StoreConfiguration> storeConfiguration)
        {
            EnsureArg.IsNotNull(contentType, nameof(contentType));
            EnsureArg.IsNotNull(body, nameof(body));
            EnsureArg.IsNotNull(seekableStreamConverter, nameof(seekableStreamConverter));
            EnsureArg.IsNotNull(storeConfiguration?.Value, nameof(storeConfiguration));

            _seekableStreamConverter = seekableStreamConverter;
            _storeConfiguration = storeConfiguration;

            if (!MediaTypeHeaderValue.TryParse(contentType, out MediaTypeHeaderValue media) ||
                !media.MediaType.Equals(KnownContentTypes.MultipartRelated, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new UnsupportedMediaTypeException(
                    string.Format(CultureInfo.InvariantCulture, DicomApiResource.UnsupportedContentType, contentType));
            }

            string boundary = HeaderUtilities.RemoveQuotes(media.Boundary).ToString();

            if (string.IsNullOrWhiteSpace(boundary))
            {
                throw new UnsupportedMediaTypeException(
                    string.Format(CultureInfo.InvariantCulture, DicomApiResource.InvalidMultipartContentType, contentType));
            }

            // Check to see if the root content type was specified or not.
            if (media.Parameters != null)
            {
                foreach (NameValueHeaderValue parameter in media.Parameters)
                {
                    if (TypeParameterName.Equals(parameter.Name.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        _rootContentType = HeaderUtilities.RemoveQuotes(parameter.Value).ToString();
                    }
                    else if (StartParameterName.Equals(parameter.Name.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        // TODO: According to RFC2387 3.2, the root section can be specified by using the
                        // start parameter. For now, we will assume that the first section is the "root" section
                        // and will add support later. Throw exception in case start is specified.
                        throw new NotSupportedException(DicomApiResource.StartParameterIsNotSupported);
                    }
                }
            }

            _multipartReader = new MultipartReader(boundary, body)
            {
                // set the max length of each section in bytes
                BodyLengthLimit = _storeConfiguration.Value.MaxAllowedDicomFileSize,
            };
        }

        /// <inheritdoc />
        public async Task<MultipartBodyPart> ReadNextBodyPartAsync(CancellationToken cancellationToken)
        {
            MultipartSection section;
            try
            {
                section = await _multipartReader.ReadNextSectionAsync(cancellationToken);
            }
            catch (InvalidDataException ex)
            {
                throw new InvalidMultipartRequestException(ex.Message);
            }

            if (section == null)
            {
                return null;
            }

            try
            {
                string contentType = section.ContentType;

                if (contentType == null && _sectionIndex == 0)
                {
                    // Based on RFC2387 Section 3.1, the content type of the "root" section
                    // can be specified through the request's Content-Type header. If the content
                    // type is not specified in the section and this is the "root" section,
                    // then check to see if it was specified in the request's Content-Type.
                    contentType = _rootContentType;
                }

                _sectionIndex++;

                // The stream must be consumed before the next ReadNextSectionAsync is called.
                // Also, the stream returned by the MultipartReader is not seekable. We need to make
                // it seekable so that we can process the stream multiple times.
                return new MultipartBodyPart(
                    contentType,
                    await _seekableStreamConverter.ConvertAsync(section.Body, cancellationToken));
            }
            catch (InvalidDataException)
            {
                // This will result in bad request, we need to handle this differently when we make the processing serial.
                throw new DicomFileLengthLimitExceededException(_storeConfiguration.Value.MaxAllowedDicomFileSize);
            }
            catch (IOException)
            {
                // We can terminate here because it seems like after it encounters the IOException,
                // next ReadNextSectionAsync will also throws IOException.
                return null;
            }
        }
    }
}
