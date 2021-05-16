﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Health.DicomCast.Core.Exceptions
{
    public class RetryableException : Exception
    {
        public RetryableException()
        {
        }

        public RetryableException(Exception innerException)
            : base(DicomCastCoreResource.RetryableException, innerException)
        {
        }
    }
}
