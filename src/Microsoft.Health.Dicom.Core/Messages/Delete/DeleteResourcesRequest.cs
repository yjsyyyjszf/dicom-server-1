﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using MediatR;

namespace Microsoft.Health.Dicom.Core.Messages.Delete
{
    public class DeleteResourcesRequest : IRequest<DeleteResourcesResponse>
    {
        public DeleteResourcesRequest(string studyInstanceUid)
        {
            StudyInstanceUid = studyInstanceUid;
            ResourceType = ResourceType.Study;
        }

        public DeleteResourcesRequest(string studyInstanceUid, string seriesInstanceUid)
        {
            StudyInstanceUid = studyInstanceUid;
            SeriesInstanceUid = seriesInstanceUid;
            ResourceType = ResourceType.Series;
        }

        public DeleteResourcesRequest(string studyInstanceUid, string seriesInstanceUid, string sopInstanceUid)
        {
            StudyInstanceUid = studyInstanceUid;
            SeriesInstanceUid = seriesInstanceUid;
            SopInstanceUid = sopInstanceUid;
            ResourceType = ResourceType.Instance;
        }

        public ResourceType ResourceType { get; }

        public string StudyInstanceUid { get; }

        public string SeriesInstanceUid { get; }

        public string SopInstanceUid { get; }
    }
}
