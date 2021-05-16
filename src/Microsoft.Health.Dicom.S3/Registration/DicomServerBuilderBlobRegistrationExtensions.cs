// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using Microsoft.Extensions.Configuration;
using Microsoft.Health.Dicom.Core.Registration;
using Microsoft.Health.Dicom.S3.Features.Storage;
using Microsoft.Health.Extensions.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DicomServerBuilderBlobRegistrationExtensions
    {
        //private const string DicomServerBlobConfigurationName = "DicomWeb:DicomStore";
        //private const string DicomServerBlobConfigurationSectionName = "DicomWeb:MetadataStore";

        public static IDicomServerBuilder AddBlobStorageDataStore(this IDicomServerBuilder serverBuilder,
            IConfiguration configuration)
        {
            EnsureArg.IsNotNull(serverBuilder, nameof(serverBuilder));
            EnsureArg.IsNotNull(configuration, nameof(configuration));

            var services = serverBuilder.Services;

            services.Add<BlobFileStore>()
                .Scoped()
                .AsSelf()
                .AsImplementedInterfaces();

            services.Add<IndexDataStore>()
                .Scoped()
                .AsImplementedInterfaces();

            return serverBuilder;
        }

        public static IDicomServerBuilder AddMetadataStorageDataStore(this IDicomServerBuilder serverBuilder,
            IConfiguration configuration)
        {
            EnsureArg.IsNotNull(serverBuilder, nameof(serverBuilder));
            EnsureArg.IsNotNull(configuration, nameof(configuration));

            return serverBuilder;
        }
    }
}
