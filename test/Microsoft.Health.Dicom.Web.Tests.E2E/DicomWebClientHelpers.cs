﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Health.Dicom.Client;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace Microsoft.Health.Dicom.Web.Tests.E2E
{
    public static class DicomWebClientHelpers
    {
        public static async Task ValidateResponseStatusCodeAsync(this IDicomWebClient dicomWebClient, Uri requestUri, string acceptHeader, HttpStatusCode expectedStatusCode)
        {
            EnsureArg.IsNotNull(dicomWebClient, nameof(dicomWebClient));
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Add(HeaderNames.Accept, acceptHeader);

            using HttpResponseMessage response = await dicomWebClient.HttpClient.SendAsync(request);

            Assert.Equal(expectedStatusCode, response.StatusCode);
        }
    }
}
