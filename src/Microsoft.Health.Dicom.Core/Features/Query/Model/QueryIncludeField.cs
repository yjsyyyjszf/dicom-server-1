﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using Dicom;

namespace Microsoft.Health.Dicom.Core.Features.Query
{
    public class QueryIncludeField
    {
        public QueryIncludeField(bool all, IReadOnlyCollection<DicomTag> dicomTags)
        {
            All = all;
            DicomTags = dicomTags;
        }

        /// <summary>
        /// If true, include all default and additional fields
        /// DicomTags are ignored if all is true
        /// </summary>
        public bool All { get; }

        /// <summary>
        /// List of additional DicomTags to return with defaults. Used only if "all=false"
        /// </summary>
        public IReadOnlyCollection<DicomTag> DicomTags { get; }
    }
}
