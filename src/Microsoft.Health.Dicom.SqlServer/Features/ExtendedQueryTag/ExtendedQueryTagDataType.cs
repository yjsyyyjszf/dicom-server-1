﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Dicom.SqlServer.Features.ExtendedQueryTag
{
    /// <summary>
    /// Date type of extended query tag.
    /// </summary>
    internal enum ExtendedQueryTagDataType
    {
        /// <summary>
        /// The extended query tag is treated as String.
        /// </summary>
        StringData = 0,

        /// <summary>
        /// The extended query tag is treated as Long.
        /// </summary>
        LongData = 1,

        /// <summary>
        /// The extended query tag is treated as Double.
        /// </summary>
        DoubleData = 2,

        /// <summary>
        /// The extended query tag is treated as DateTime.
        /// </summary>
        DateTimeData = 3,

        /// <summary>
        /// The extended query tag is treated as PersonName.
        /// </summary>
        PersonNameData = 4,
    }
}
