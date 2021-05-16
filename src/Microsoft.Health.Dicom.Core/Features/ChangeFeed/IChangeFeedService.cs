﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Health.Dicom.Core.Features.ChangeFeed
{
    public interface IChangeFeedService
    {
        public Task<IReadOnlyCollection<ChangeFeedEntry>> GetChangeFeedAsync(long offset, int limit, bool includeMetadata, CancellationToken cancellationToken = default);

        public Task<ChangeFeedEntry> GetChangeFeedLatestAsync(bool includeMetadata, CancellationToken cancellationToken = default);
    }
}
