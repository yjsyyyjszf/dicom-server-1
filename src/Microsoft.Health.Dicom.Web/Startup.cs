﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Development.IdentityProvider.Registration;
using Microsoft.Health.Dicom.Core.Features.Security;

namespace Microsoft.Health.Dicom.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.Configure<IISServerOptions>(options =>
            {
                // When hosted on IIS, the max request body size can not over 2GB, according to Asp.net Core bug https://github.com/dotnet/aspnetcore/issues/2711
                options.MaxRequestBodySize = int.MaxValue;
            });
            services.AddDevelopmentIdentityProvider<DataActions>(Configuration, "DicomServer");

            services.AddDicomServer(Configuration)
                .AddBlobStorageDataStore(Configuration)
                .AddMetadataStorageDataStore(Configuration)
                .ConfigureS3(Configuration);
            // Instead of registering the old Azure and SQL Server based services, 
            //.AddSqlServer(Configuration)
            /*
                The execution of IHostedServices depends on the order they are added to the dependency injection container, so we
                need to ensure that the schema is initialized before the background workers are started.
            */
            //.AddBackgroundWorkers();

            AddApplicationInsightsTelemetry(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public virtual void Configure(IApplicationBuilder app)
        {
            app.UseDicomServer();

            app.UseDevelopmentIdentityProviderIfConfigured();
        }

        /// <summary>
        /// Adds ApplicationInsights for telemetry and logging.
        /// </summary>
        private void AddApplicationInsightsTelemetry(IServiceCollection services)
        {
            string instrumentationKey = Configuration["ApplicationInsights:InstrumentationKey"];

            if (!string.IsNullOrWhiteSpace(instrumentationKey))
            {
                services.AddApplicationInsightsTelemetry(instrumentationKey);
                services.AddLogging(loggingBuilder => loggingBuilder.AddApplicationInsights(instrumentationKey));
            }
        }
    }
}
