﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Hl7.Fhir.Model;
using Microsoft.Health.DicomCast.Core.Features.Fhir;
using Microsoft.Health.DicomCast.Core.Features.Worker.FhirTransaction;
using Xunit;

namespace Microsoft.Health.DicomCast.Core.UnitTests.Features.Worker.FhirTransaction
{
    public static class ValidationUtility
    {
        public static void ValidateRequestEntryMinimumRequirementForWithChange(
            FhirTransactionRequestMode expectedRequestMode,
            string path,
            Bundle.HTTPVerb? expectedMethod,
            FhirTransactionRequestEntry actualEntry)
        {
            // For request entry with no change, use the method below.
            Assert.NotEqual(FhirTransactionRequestMode.None, expectedRequestMode);

            Assert.NotNull(actualEntry);
            Assert.Equal(expectedRequestMode, actualEntry.RequestMode);

            if (expectedRequestMode == FhirTransactionRequestMode.Create)
            {
                // If the request mode is create, then it should be client generated resource id.
                Assert.IsType<ClientResourceId>(actualEntry.ResourceId);
            }
            else if (expectedRequestMode == FhirTransactionRequestMode.Update)
            {
                // Otherwise, it should be server generated resource id.
                ServerResourceId serverResourceId = Assert.IsType<ServerResourceId>(actualEntry.ResourceId);

                Assert.Equal(path, serverResourceId.ToString());
            }

            Assert.NotNull(actualEntry.Request);
            Assert.Equal(expectedMethod, actualEntry.Request.Method);
            Assert.Equal(path, actualEntry.Request.Url);

            if (expectedMethod != Bundle.HTTPVerb.DELETE)
            {
                Assert.NotNull(actualEntry.Resource);
            }
        }

        public static void ValidateRequestEntryMinimumRequirementForNoChange(ServerResourceId expectedResourceId, FhirTransactionRequestEntry actualEntry)
        {
            Assert.NotNull(actualEntry);
            Assert.Equal(FhirTransactionRequestMode.None, actualEntry.RequestMode);

            // No update means the resource already exists and nothing has changed,
            // so it should still have server generated resource id.
            ServerResourceId serverResourceId = Assert.IsType<ServerResourceId>(actualEntry.ResourceId);

            Assert.Equal(expectedResourceId, serverResourceId);

            Assert.Null(actualEntry.Request);
        }

        public static void ValidateIdentifier(string expectedSystem, string expectedValue, Identifier actualIdentifier)
        {
            Assert.NotNull(actualIdentifier);
            Assert.Equal(expectedSystem, actualIdentifier.System);
            Assert.Equal(expectedValue, actualIdentifier.Value);
        }

        public static void ValidateAccessionNumber(string expectedSystem, string expectedValue, Identifier actualIdentifier)
        {
            Assert.NotNull(actualIdentifier);
            Assert.Equal(actualIdentifier.Value, expectedValue);
            Assert.Equal(actualIdentifier.System, expectedSystem);
            Assert.Single(actualIdentifier.Type.Coding);
            Assert.Equal("http://terminology.hl7.org/CodeSystem/v2-0203", actualIdentifier.Type.Coding[0].System);
            Assert.Equal("ACSN", actualIdentifier.Type.Coding[0].Code);
        }

        public static void ValidateResourceReference(string expectedReference, ResourceReference actualResourceReference)
        {
            Assert.NotNull(actualResourceReference);
            Assert.Equal(expectedReference, actualResourceReference.Reference);
        }

        public static void ValidateSeries(ImagingStudy.SeriesComponent series, string seriesInstanceUid, params string[] sopInstanceUidList)
        {
            Assert.Equal(seriesInstanceUid, series.Uid);

            for (int i = 0; i < series.Instance.Count; i++)
            {
                Assert.Equal(sopInstanceUidList[i], series.Instance[i].Uid);
            }
        }

        public static ImagingStudy ValidateImagingStudyUpdate(string studyInstanceUid, string patientResourceId, FhirTransactionRequestEntry entry, bool hasAccessionNumber = true)
        {
            Identifier expectedIdentifier = ImagingStudyIdentifierUtility.CreateIdentifier(studyInstanceUid);
            string expectedRequestUrl = $"ImagingStudy/{entry.Resource.Id}";

            ValidateRequestEntryMinimumRequirementForWithChange(FhirTransactionRequestMode.Update, expectedRequestUrl, Bundle.HTTPVerb.PUT, actualEntry: entry);

            ImagingStudy updatedImagingStudy = Assert.IsType<ImagingStudy>(entry.Resource);

            Assert.Equal(ImagingStudy.ImagingStudyStatus.Available, updatedImagingStudy.Status);

            ValidateResourceReference(patientResourceId, updatedImagingStudy.Subject);

            Action<Identifier> studyIdValidaion = identifier => ValidateIdentifier("urn:dicom:uid", $"urn:oid:{studyInstanceUid}", identifier);
            Action<Identifier> accessionNumberValidation = identifier => ValidateAccessionNumber(null, FhirTransactionContextBuilder.DefaultAccessionNumber, identifier);
            Assert.Collection(
                updatedImagingStudy.Identifier,
                hasAccessionNumber ? new[] { studyIdValidaion, accessionNumberValidation } : new[] { studyIdValidaion });

            return updatedImagingStudy;
        }
    }
}
