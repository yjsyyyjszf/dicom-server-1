﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------
using Microsoft.Health.Dicom.Core.Features.ExtendedQueryTag;

namespace Microsoft.Health.Dicom.Core.Features.Query
{
    public abstract class RangeValueMatchCondition<T> : QueryFilterCondition
    {
        internal RangeValueMatchCondition(QueryTag tag, T minimum, T maximum)
            : base(tag)
        {
            Minimum = minimum;
            Maximum = maximum;
        }

        public T Minimum { get; set; }

        public T Maximum { get; set; }
    }
}
