﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Health.DicomCast.TableStorage
{
    internal static class Constants
    {
        public const string FhirExceptionTableName = "FhirFailToStoreExceptionTable";

        public const string DicomExceptionTableName = "DicomFailToStoreExceptionTable";

        public const string DicomValidationTableName = "InvalidDicomTagExceptionTable";

        public const string TransientFailureTableName = "TransientFailureExceptionTable";

        public const string TransientRetryTableName = "TransientRetryExceptionTable";

        public const string SyncStateTableName = "SyncStateTable";

        public const string SyncStatePartitionKey = "SyncStatePartitionKey";

        public const string SyncStateRowKey = "SyncStateRowKey";

        // List of all the tables that need to be initialized
        public static readonly List<string> AllTables = new List<string> { SyncStateTableName, FhirExceptionTableName, DicomExceptionTableName, TransientFailureTableName, DicomValidationTableName, TransientRetryTableName };
    }
}
