﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Dicom.Core.Features.ExtendedQueryTag
{
    /// <summary>
    /// Level of a query tag.
    /// </summary>
    public enum QueryTagLevel
    {
        /// <summary>
        /// The query tag is on instance level.
        /// </summary>
        Instance = 0,

        /// <summary>
        /// The query tag is on series level.
        /// </summary>
        Series = 1,

        /// <summary>
        /// The query tag is on study level.
        /// </summary>
        Study = 2,
    }
}
