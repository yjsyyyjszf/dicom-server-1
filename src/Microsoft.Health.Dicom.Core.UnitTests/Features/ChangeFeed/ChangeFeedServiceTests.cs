﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Health.Dicom.Core.Features.ChangeFeed;
using Microsoft.Health.Dicom.Core.Features.Common;
using Microsoft.Health.Dicom.Core.Features.Model;
using Microsoft.Health.Dicom.Tests.Common;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Dicom.Core.UnitTests.Features.ChangeFeed
{
    public class ChangeFeedServiceTests
    {
        private readonly IChangeFeedStore _changeFeedStore;
        private readonly IMetadataStore _metadataStore;
        private readonly ChangeFeedService _changeFeedService;
        private List<ChangeFeedEntry> _changeFeedEntries;

        public ChangeFeedServiceTests()
        {
            _changeFeedEntries = new List<ChangeFeedEntry>
            {
                new ChangeFeedEntry(1, DateTime.Now, ChangeFeedAction.Create, TestUidGenerator.Generate(), TestUidGenerator.Generate(), TestUidGenerator.Generate(), 1, 1, ChangeFeedState.Current),
                new ChangeFeedEntry(2, DateTime.Now, ChangeFeedAction.Create, TestUidGenerator.Generate(), TestUidGenerator.Generate(), TestUidGenerator.Generate(), 2, 2, ChangeFeedState.Current),
                new ChangeFeedEntry(3, DateTime.Now, ChangeFeedAction.Create, TestUidGenerator.Generate(), TestUidGenerator.Generate(), TestUidGenerator.Generate(), 3, 3, ChangeFeedState.Current),
                new ChangeFeedEntry(4, DateTime.Now, ChangeFeedAction.Create, TestUidGenerator.Generate(), TestUidGenerator.Generate(), TestUidGenerator.Generate(), 4, 4, ChangeFeedState.Current),
                new ChangeFeedEntry(5, DateTime.Now, ChangeFeedAction.Create, TestUidGenerator.Generate(), TestUidGenerator.Generate(), TestUidGenerator.Generate(), 5, 5, ChangeFeedState.Current),
                new ChangeFeedEntry(6, DateTime.Now, ChangeFeedAction.Create, TestUidGenerator.Generate(), TestUidGenerator.Generate(), TestUidGenerator.Generate(), 6, 6, ChangeFeedState.Current),
                new ChangeFeedEntry(7, DateTime.Now, ChangeFeedAction.Create, TestUidGenerator.Generate(), TestUidGenerator.Generate(), TestUidGenerator.Generate(), 7, 7, ChangeFeedState.Current),
                new ChangeFeedEntry(8, DateTime.Now, ChangeFeedAction.Create, TestUidGenerator.Generate(), TestUidGenerator.Generate(), TestUidGenerator.Generate(), 8, 8, ChangeFeedState.Current),
                new ChangeFeedEntry(9, DateTime.Now, ChangeFeedAction.Create, TestUidGenerator.Generate(), TestUidGenerator.Generate(), TestUidGenerator.Generate(), 9, 9, ChangeFeedState.Current),
                new ChangeFeedEntry(10, DateTime.Now, ChangeFeedAction.Create, TestUidGenerator.Generate(), TestUidGenerator.Generate(), TestUidGenerator.Generate(), 10, 10, ChangeFeedState.Current),
            };
            _changeFeedStore = Substitute.For<IChangeFeedStore>();

            _changeFeedStore.GetChangeFeedAsync(offset: default, limit: default, cancellationToken: default)
                .ReturnsForAnyArgs(callInfo =>
                {
                    return _changeFeedEntries?.Skip((int)callInfo.ArgAt<long>(0)).Take(callInfo.ArgAt<int>(1)).ToList();
                });

            _changeFeedStore.GetChangeFeedLatestAsync(cancellationToken: default)
                .ReturnsForAnyArgs(callInfo => _changeFeedEntries.Last());

            _metadataStore = Substitute.For<IMetadataStore>();
            _changeFeedService = new ChangeFeedService(_changeFeedStore, _metadataStore);
        }

        [InlineData(0, 10, true)]
        [InlineData(0, 10, false)]
        [InlineData(3, 10, true)]
        [Theory]
        public async Task GivenAChangeFeedRequest_WhenGetChangeFeedCalled_ThenUnderlyingStoresAreCalled(int offset, int limit, bool includeMetadata)
        {
            IReadOnlyCollection<ChangeFeedEntry> results = await _changeFeedService.GetChangeFeedAsync(offset, limit, includeMetadata, CancellationToken.None);

            await _changeFeedStore.Received(1).GetChangeFeedAsync(offset, limit, CancellationToken.None);

            if (includeMetadata)
            {
                await _metadataStore.Received(limit - offset).GetInstanceMetadataAsync(Arg.Any<VersionedInstanceIdentifier>(), default);
            }
            else
            {
                await _metadataStore.DidNotReceiveWithAnyArgs().GetInstanceMetadataAsync(Arg.Any<VersionedInstanceIdentifier>(), default);
            }

            Assert.Equal(_changeFeedEntries.Skip(offset).Take(limit), results);
        }

        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public async Task GivenAChangeFeedLatestRequest_WhenGetChangeFeedLatestCalled_ThenUnderlyingStoresAreCalled(bool includeMetadata)
        {
            ChangeFeedEntry result = await _changeFeedService.GetChangeFeedLatestAsync(includeMetadata, CancellationToken.None);

            await _changeFeedStore.Received(1).GetChangeFeedLatestAsync(CancellationToken.None);

            if (includeMetadata)
            {
                await _metadataStore.Received(1).GetInstanceMetadataAsync(Arg.Any<VersionedInstanceIdentifier>(), default);
            }
            else
            {
                await _metadataStore.DidNotReceiveWithAnyArgs().GetInstanceMetadataAsync(Arg.Any<VersionedInstanceIdentifier>(), default);
            }

            Assert.Equal(_changeFeedEntries.Last(), result);
        }

        [Fact]
        public async Task GivenAnEmptyChangeFeed_WhenGetChangeFeedCalled_ThenEmptyCollectionIsReturned()
        {
            _changeFeedEntries = Enumerable.Empty<ChangeFeedEntry>().ToList();

            IReadOnlyCollection<ChangeFeedEntry> results = await _changeFeedService.GetChangeFeedAsync(0, 10, true, CancellationToken.None);

            await _changeFeedStore.Received(1).GetChangeFeedAsync(0, 10, CancellationToken.None);
            await _metadataStore.DidNotReceiveWithAnyArgs().GetInstanceMetadataAsync(Arg.Any<VersionedInstanceIdentifier>(), default);

            Assert.Empty(results);
        }

        [Fact]
        public async Task GivenAnEmptyChangeFeed_WhenGetChangeFeedLatestCalled_ThenNullIsReturned()
        {
            _changeFeedStore.GetChangeFeedLatestAsync(default)
                .ReturnsForAnyArgs((ChangeFeedEntry)null);

            ChangeFeedEntry result = await _changeFeedService.GetChangeFeedLatestAsync(true, CancellationToken.None);

            await _changeFeedStore.Received(1).GetChangeFeedLatestAsync(CancellationToken.None);
            await _metadataStore.DidNotReceiveWithAnyArgs().GetInstanceMetadataAsync(Arg.Any<VersionedInstanceIdentifier>(), default);

            Assert.Null(result);
        }

        [Fact]
        public async Task GivenADeletedChangeFeedEntry_WhenGetChangeFeedCalled_ThenUnderlyingStoresAreCalled()
        {
            AddDeletedEntry();

            IReadOnlyCollection<ChangeFeedEntry> results = await _changeFeedService.GetChangeFeedAsync(1, 10, true, CancellationToken.None);

            await _changeFeedStore.Received(1).GetChangeFeedAsync(1, 10, CancellationToken.None);
            await _metadataStore.Received(9).GetInstanceMetadataAsync(Arg.Any<VersionedInstanceIdentifier>(), default);

            Assert.Equal(_changeFeedEntries.Skip(1).Take(10), results);
        }

        [Fact]
        public async Task GivenADeletedChangeFeedEntry_WhenGetChangeFeedLatestCalled_ThenUnderlyingStoresAreCalled()
        {
            AddDeletedEntry();

            ChangeFeedEntry result = await _changeFeedService.GetChangeFeedLatestAsync(true, CancellationToken.None);

            await _changeFeedStore.Received(1).GetChangeFeedLatestAsync(CancellationToken.None);
            await _metadataStore.DidNotReceiveWithAnyArgs().GetInstanceMetadataAsync(Arg.Any<VersionedInstanceIdentifier>(), default);

            Assert.Equal(_changeFeedEntries.Last(), result);
        }

        private void AddDeletedEntry()
        {
            _changeFeedEntries.Add(
                new ChangeFeedEntry(11, DateTime.Now, ChangeFeedAction.Delete, TestUidGenerator.Generate(), TestUidGenerator.Generate(), TestUidGenerator.Generate(), 11, null, ChangeFeedState.Deleted));
        }
    }
}
