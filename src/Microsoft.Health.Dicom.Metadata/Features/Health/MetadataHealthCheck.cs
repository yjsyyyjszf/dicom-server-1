﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Health.Blob.Configs;
using Microsoft.Health.Blob.Features.Health;
using Microsoft.Health.Blob.Features.Storage;

namespace Microsoft.Health.Dicom.Metadata.Features.Health
{
    /// <summary>
    /// Checks for the DICOM metadata service health.
    /// </summary>
    public class MetadataHealthCheck : BlobHealthCheck
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataHealthCheck"/> class.
        /// </summary>
        /// <param name="client">The blob client factory.</param>
        /// <param name="configuration">The blob data store configuration.</param>
        /// <param name="namedBlobContainerConfigurationAccessor">The IOptions accessor to get a named blob container version.</param>
        /// <param name="testProvider">The test provider.</param>
        /// <param name="logger">The logger.</param>
        public MetadataHealthCheck(
            BlobServiceClient client,
            BlobDataStoreConfiguration configuration,
            IOptionsSnapshot<BlobContainerConfiguration> namedBlobContainerConfigurationAccessor,
            IBlobClientTestProvider testProvider,
            ILogger<MetadataHealthCheck> logger)
            : base(
                  client,
                  configuration,
                  namedBlobContainerConfigurationAccessor,
                  Constants.ContainerConfigurationName,
                  testProvider,
                  logger)
        {
        }
    }
}
