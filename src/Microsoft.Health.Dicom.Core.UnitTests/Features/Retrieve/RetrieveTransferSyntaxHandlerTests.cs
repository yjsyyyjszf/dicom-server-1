﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Dicom;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Health.Dicom.Core.Exceptions;
using Microsoft.Health.Dicom.Core.Features.Retrieve;
using Microsoft.Health.Dicom.Core.Messages;
using Microsoft.Health.Dicom.Core.Messages.Retrieve;
using Microsoft.Health.Dicom.Core.Web;
using Microsoft.Health.Dicom.Tests.Common;
using Xunit;

namespace Microsoft.Health.Dicom.Core.UnitTests.Features.Retrieve
{
    public class RetrieveTransferSyntaxHandlerTests
    {
        private readonly RetrieveTransferSyntaxHandler _handler;

        public RetrieveTransferSyntaxHandlerTests()
        {
            _handler = new RetrieveTransferSyntaxHandler(NullLogger<RetrieveTransferSyntaxHandler>.Instance);
        }

        [Fact(Skip = "Will be enabled later as https://microsofthealth.visualstudio.com/Health/_workitems/edit/75782")]
        public void GivenMultipleMatchedAcceptHeadersWithDifferentQuality_WhenGetTransferSyntax_ThenShouldReturnLargestQuality()
        {
            string expectedTransferSyntax = DicomTransferSyntax.ExplicitVRLittleEndian.UID.UID;
            AcceptHeader acceptHeader1 = AcceptHeaderHelpers.CreateAcceptHeaderForGetFrame(quality: 0.5, transferSyntax: DicomTransferSyntaxUids.Original);
            AcceptHeader acceptHeader2 = AcceptHeaderHelpers.CreateAcceptHeaderForGetFrame(quality: 0.9, transferSyntax: expectedTransferSyntax);
            AcceptHeaderDescriptor acceptHeaderDescriptor;
            string transferSyntax = _handler.GetTransferSyntax(ResourceType.Frames, new[] { acceptHeader1, acceptHeader2 }, out acceptHeaderDescriptor);
            Assert.Equal(expectedTransferSyntax, transferSyntax);
            Assert.Equal(acceptHeader2.MediaType, acceptHeaderDescriptor.MediaType);
        }

        [Fact]
        public void GivenNoMatchedAcceptHeaders_WhenGetTransferSyntax_ThenShouldThrowNotAcceptableException()
        {
            // Use content type that GetStudy doesn't support
            AcceptHeader acceptHeader = AcceptHeaderHelpers.CreateAcceptHeaderForGetStudy(mediaType: KnownContentTypes.ImageJpeg);
            AcceptHeaderDescriptor acceptHeaderDescriptor;
            Assert.ThrowsAny<NotAcceptableException>(() => _handler.GetTransferSyntax(ResourceType.Study, new[] { acceptHeader }, out acceptHeaderDescriptor));
        }

        [Fact]
        public void GivenMultipleAcceptHeaders_WhenGetTransferSyntax_ThenShouldThrowNotAcceptableException()
        {
            AcceptHeader acceptHeader1 = AcceptHeaderHelpers.CreateAcceptHeaderForGetFrame(quality: 0.5, transferSyntax: DicomTransferSyntaxUids.Original);
            AcceptHeader acceptHeader2 = AcceptHeaderHelpers.CreateAcceptHeaderForGetFrame(quality: 0.9, transferSyntax: DicomTransferSyntaxUids.Original);
            AcceptHeaderDescriptor acceptHeaderDescriptor;
            Assert.ThrowsAny<NotAcceptableException>(() => _handler.GetTransferSyntax(ResourceType.Study, new[] { acceptHeader1, acceptHeader2 }, out acceptHeaderDescriptor));
        }
    }
}
