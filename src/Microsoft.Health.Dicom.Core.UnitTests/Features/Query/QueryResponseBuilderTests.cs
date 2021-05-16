﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Dicom;
using Microsoft.Health.Dicom.Core.Features.ExtendedQueryTag;
using Microsoft.Health.Dicom.Core.Features.Query;
using Microsoft.Health.Dicom.Core.Features.Query.Model;
using Microsoft.Health.Dicom.Tests.Common;
using Microsoft.Health.Dicom.Tests.Common.Extensions;
using Xunit;

namespace Microsoft.Health.Dicom.Core.UnitTests.Features.Query
{
    public class QueryResponseBuilderTests
    {
        [Fact]
        public void GivenStudyLevel_WithIncludeField_ValidReturned()
        {
            var includeField = new QueryIncludeField(false, new List<DicomTag>() { DicomTag.StudyDescription, DicomTag.IssuerOfPatientID });
            var queryTag = new QueryTag(DicomTag.PatientAge.BuildExtendedQueryTagStoreEntry(level: QueryTagLevel.Study));
            var filters = new List<QueryFilterCondition>()
            {
                new StringSingleValueMatchCondition(queryTag, "35"),
            };
            var query = new QueryExpression(QueryResource.AllStudies, includeField, false, 0, 0, filters);
            var responseBuilder = new QueryResponseBuilder(query);

            DicomDataset responseDataset = responseBuilder.GenerateResponseDataset(GenerateTestDataSet());
            var tags = responseDataset.Select(i => i.Tag).ToList();

            Assert.Contains<DicomTag>(DicomTag.StudyInstanceUID, tags); // Default
            Assert.Contains<DicomTag>(DicomTag.PatientAge, tags); // Match condition
            Assert.Contains<DicomTag>(DicomTag.StudyDescription, tags); // Valid include
            Assert.Contains<DicomTag>(DicomTag.IssuerOfPatientID, tags); // non standard include
            Assert.DoesNotContain<DicomTag>(DicomTag.SeriesInstanceUID, tags); // Invalid study resource
            Assert.DoesNotContain<DicomTag>(DicomTag.SOPInstanceUID, tags); // Invalid study resource
        }

        [Fact]
        public void GivenStudySeriesLevel_WithIncludeField_ValidReturned()
        {
            var includeField = new QueryIncludeField(false, new List<DicomTag>() { DicomTag.StudyDescription, DicomTag.Modality });
            var queryTag = new QueryTag(DicomTag.StudyInstanceUID.BuildExtendedQueryTagStoreEntry(level: QueryTagLevel.Study));
            var filters = new List<QueryFilterCondition>()
            {
                new StringSingleValueMatchCondition(queryTag, "35"),
            };
            var query = new QueryExpression(QueryResource.StudySeries, includeField, false, 0, 0, filters);
            var responseBuilder = new QueryResponseBuilder(query);

            DicomDataset responseDataset = responseBuilder.GenerateResponseDataset(GenerateTestDataSet());
            var tags = responseDataset.Select(i => i.Tag).ToList();

            Assert.Contains<DicomTag>(DicomTag.StudyInstanceUID, tags); // Valid filter
            Assert.Contains<DicomTag>(DicomTag.StudyDescription, tags); // Valid include
            Assert.Contains<DicomTag>(DicomTag.Modality, tags); // Valid include
            Assert.Contains<DicomTag>(DicomTag.SeriesInstanceUID, tags); // Valid Series resource
            Assert.DoesNotContain<DicomTag>(DicomTag.SOPInstanceUID, tags); // Invalid Series resource
        }

        [Fact]
        public void GivenAllSeriesLevel_WithIncludeField_ValidReturned()
        {
            var includeField = new QueryIncludeField(true, new List<DicomTag>() { });
            var filters = new List<QueryFilterCondition>();
            var query = new QueryExpression(QueryResource.AllSeries, includeField, false, 0, 0, filters);
            var responseBuilder = new QueryResponseBuilder(query);

            DicomDataset responseDataset = responseBuilder.GenerateResponseDataset(GenerateTestDataSet());
            var tags = responseDataset.Select(i => i.Tag).ToList();

            Assert.Contains<DicomTag>(DicomTag.StudyInstanceUID, tags); // Valid study field
            Assert.Contains<DicomTag>(DicomTag.StudyDescription, tags); // Valid all study field
            Assert.Contains<DicomTag>(DicomTag.Modality, tags); // Valid series field
            Assert.Contains<DicomTag>(DicomTag.SeriesInstanceUID, tags); // Valid Series resource
            Assert.DoesNotContain<DicomTag>(DicomTag.SOPInstanceUID, tags); // Invalid Series resource
        }

        [Fact]
        public void GivenAllInstanceLevel_WithIncludeField_ValidReturned()
        {
            var includeField = new QueryIncludeField(true, new List<DicomTag>() { });
            var filters = new List<QueryFilterCondition>();
            var query = new QueryExpression(QueryResource.AllInstances, includeField, false, 0, 0, filters);
            var responseBuilder = new QueryResponseBuilder(query);

            DicomDataset responseDataset = responseBuilder.GenerateResponseDataset(GenerateTestDataSet());
            var tags = responseDataset.Select(i => i.Tag).ToList();

            Assert.Contains<DicomTag>(DicomTag.StudyInstanceUID, tags); // Valid study field
            Assert.Contains<DicomTag>(DicomTag.StudyDescription, tags); // Valid all study field
            Assert.Contains<DicomTag>(DicomTag.Modality, tags); // Valid instance field
            Assert.Contains<DicomTag>(DicomTag.SeriesInstanceUID, tags); // Valid instance resource
            Assert.Contains<DicomTag>(DicomTag.SOPInstanceUID, tags); // Valid instance resource
        }

        [Fact]
        public void GivenStudyInstanceLevel_WithIncludeField_ValidReturned()
        {
            var includeField = new QueryIncludeField(false, new List<DicomTag>() { DicomTag.Modality });
            var filters = new List<QueryFilterCondition>()
            {
                new StringSingleValueMatchCondition(new QueryTag(DicomTag.StudyInstanceUID), "35"),
            };
            var query = new QueryExpression(QueryResource.StudyInstances, includeField, false, 0, 0, filters);
            var responseBuilder = new QueryResponseBuilder(query);

            DicomDataset responseDataset = responseBuilder.GenerateResponseDataset(GenerateTestDataSet());
            var tags = responseDataset.Select(i => i.Tag).ToList();

            Assert.Contains<DicomTag>(DicomTag.StudyInstanceUID, tags); // Valid filter
            Assert.DoesNotContain<DicomTag>(DicomTag.StudyDescription, tags); // StudyInstance does not include study tags by deault
            Assert.Contains<DicomTag>(DicomTag.Modality, tags); // Valid series field
            Assert.Contains<DicomTag>(DicomTag.SeriesInstanceUID, tags); // Valid series tag
            Assert.Contains<DicomTag>(DicomTag.SOPInstanceUID, tags); // Valid instance tag
        }

        [Fact]
        public void GivenStudySeriesInstanceLevel_WithIncludeField_ValidReturned()
        {
            var includeField = new QueryIncludeField(false, new List<DicomTag>() { });

            var filters = new List<QueryFilterCondition>()
            {
                new StringSingleValueMatchCondition(new QueryTag(DicomTag.StudyInstanceUID), "35"),
                new StringSingleValueMatchCondition(new QueryTag(DicomTag.SeriesInstanceUID), "351"),
            };
            var query = new QueryExpression(QueryResource.StudySeriesInstances, includeField, false, 0, 0, filters);
            var responseBuilder = new QueryResponseBuilder(query);

            DicomDataset responseDataset = responseBuilder.GenerateResponseDataset(GenerateTestDataSet());
            var tags = responseDataset.Select(i => i.Tag).ToList();

            Assert.Contains<DicomTag>(DicomTag.StudyInstanceUID, tags); // Valid filter
            Assert.DoesNotContain<DicomTag>(DicomTag.StudyDescription, tags); // StudySeriesInstance does not include study tags by deault
            Assert.DoesNotContain<DicomTag>(DicomTag.Modality, tags); // StudySeriesInstance does not include series tags by deault
            Assert.Contains<DicomTag>(DicomTag.SeriesInstanceUID, tags); // Valid series tag
            Assert.Contains<DicomTag>(DicomTag.SOPInstanceUID, tags); // Valid instance tag
        }

        private DicomDataset GenerateTestDataSet()
        {
            return new DicomDataset()
            {
                { DicomTag.StudyInstanceUID, TestUidGenerator.Generate() },
                { DicomTag.SeriesInstanceUID, TestUidGenerator.Generate() },
                { DicomTag.SOPInstanceUID, TestUidGenerator.Generate() },
                { DicomTag.PatientAge, "035Y" },
                { DicomTag.StudyDescription, "CT scan" },
                { DicomTag.IssuerOfPatientID, "Homeland" },
                { DicomTag.Modality, "CT" },
            };
        }
    }
}
