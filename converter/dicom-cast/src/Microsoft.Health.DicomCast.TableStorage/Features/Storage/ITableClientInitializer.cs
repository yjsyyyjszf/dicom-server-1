﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Health.DicomCast.TableStorage.Configs;

namespace Microsoft.Health.DicomCast.TableStorage.Features.Storage
{
    /// <summary>
    /// Provides methods for creating a TableClient instance and initializing tables.
    /// </summary>
    public interface ITableClientInitializer
    {
        /// <summary>
        /// Creates <see cref="CloudTableClient"/> based on the given <see cref="TableDataStoreConfiguration"/>.
        /// </summary>
        /// <returns>A <see cref="CloudTableClient"/> instance</returns>
        CloudTableClient CreateTableClient();

        /// <summary>
        /// Initialize table data store
        /// </summary>
        /// <param name="client">The <see cref="CloudTableClient"/> instance to use for initialization.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        Task IntializeDataStoreAsync(CloudTableClient client);
    }
}
