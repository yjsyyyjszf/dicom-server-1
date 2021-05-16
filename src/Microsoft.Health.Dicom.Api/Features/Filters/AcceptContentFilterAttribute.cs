﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using EnsureThat;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using Microsoft.Health.Dicom.Core.Web;
using Microsoft.Net.Http.Headers;

namespace Microsoft.Health.Dicom.Api.Features.Filters
{
    public sealed class AcceptContentFilterAttribute : ActionFilterAttribute
    {
        private const int NotAcceptableResponseCode = (int)HttpStatusCode.NotAcceptable;
        private const string TypeParameter = "type";

        private readonly bool _allowSingle;
        private readonly bool _allowMultiple;

        private readonly HashSet<MediaTypeHeaderValue> _mediaTypes;

        public AcceptContentFilterAttribute(string[] mediaTypes, bool allowSingle, bool allowMultiple)
        {
            EnsureArg.IsNotNull(mediaTypes, nameof(mediaTypes));
            Debug.Assert(mediaTypes.Length > 0, "The accept content type filter must have at least one media type specified.");

            _mediaTypes = new HashSet<MediaTypeHeaderValue>(mediaTypes.Length);

            foreach (var mediaType in mediaTypes)
            {
                if (MediaTypeHeaderValue.TryParse(mediaType, out MediaTypeHeaderValue parsedMediaType))
                {
                    _mediaTypes.Add(parsedMediaType);
                }
                else
                {
                    Debug.Assert(false, "The values in the mediaTypes parameter must be parseable by MediaTypeHeaderValue.");
                }
            }

            _allowSingle = allowSingle;
            _allowMultiple = allowMultiple;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            EnsureArg.IsNotNull(context, nameof(context));
            IList<MediaTypeHeaderValue> acceptHeaders = context.HttpContext.Request.GetTypedHeaders().Accept;

            bool acceptable = false;

            // Validate the accept headers has one of the specified accepted media types.
            if (acceptHeaders != null && acceptHeaders.Count > 0)
            {
                foreach (MediaTypeHeaderValue acceptHeader in acceptHeaders)
                {
                    if (_allowMultiple && StringSegment.Equals(acceptHeader.MediaType, KnownContentTypes.MultipartRelated, StringComparison.InvariantCultureIgnoreCase))
                    {
                        NameValueHeaderValue typeParameterValue = acceptHeader.Parameters.FirstOrDefault(
                            parameter => StringSegment.Equals(parameter.Name, TypeParameter, StringComparison.InvariantCultureIgnoreCase));

                        if (typeParameterValue != null &&
                            MediaTypeHeaderValue.TryParse(HeaderUtilities.RemoveQuotes(typeParameterValue.Value), out MediaTypeHeaderValue parsedValue) &&
                            _mediaTypes.Contains(parsedValue))
                        {
                            acceptable = true;
                            break;
                        }
                    }

                    if (_allowSingle)
                    {
                        string[] split = acceptHeader.ToString().Split(';');
                        var stringHeaders = _mediaTypes.Select(x => x.ToString()).ToList();
                        if (split.Any(x => stringHeaders.Contains(x, StringComparer.InvariantCultureIgnoreCase)))
                        {
                            acceptable = true;
                            break;
                        }
                    }
                }
            }

            if (!acceptable)
            {
                context.Result = new StatusCodeResult(NotAcceptableResponseCode);
            }

            base.OnActionExecuting(context);
        }
    }
}
