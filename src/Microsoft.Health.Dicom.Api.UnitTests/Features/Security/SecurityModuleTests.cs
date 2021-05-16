﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Dicom.Api.Configs;
using Microsoft.Health.Dicom.Api.Modules;
using Microsoft.Health.Dicom.Core.Configs;
using Xunit;

namespace Microsoft.Health.Dicom.Api.UnitTests.Features.Security
{
    public class SecurityModuleTests
    {
        [Fact]
        public void GivenASecurityConfigurationWithAudience_WhenGettingValidAudiences_ThenCorrectAudienceShouldBeReturned()
        {
            var dicomServerConfiguration = new DicomServerConfiguration
            {
                Security =
                {
                    Authentication = new AuthenticationConfiguration
                    {
                        Audience = "initialAudience",
                    },
                },
            };

            var securityModule = new SecurityModule(dicomServerConfiguration);

            Assert.Equal(new[] { "initialAudience" }, securityModule.GetValidAudiences());
        }

        [Fact]
        public void GivenASecurityConfigurationWithAudienceAndAudiences_WhenGettingValidAudiences_ThenCorrectAudienceShouldBeReturned()
        {
            var dicomServerConfiguration = new DicomServerConfiguration
            {
                Security =
                {
                    Authentication = new AuthenticationConfiguration
                    {
                        Audience = "initialAudience",
                        Audiences = new[] { "audience1", "audience2" },
                    },
                },
            };

            var securityModule = new SecurityModule(dicomServerConfiguration);

            Assert.Equal(new[] { "audience1", "audience2" }, securityModule.GetValidAudiences());
        }

        [Fact]
        public void GivenASecurityConfigurationWithAudiences_WhenGettingValidAudiences_ThenCorrectAudienceShouldBeReturned()
        {
            var dicomServerConfiguration = new DicomServerConfiguration
            {
                Security =
                {
                    Authentication = new AuthenticationConfiguration
                    {
                        Audiences = new[] { "audience1", "audience2" },
                    },
                },
            };

            var securityModule = new SecurityModule(dicomServerConfiguration);

            Assert.Equal(new[] { "audience1", "audience2" }, securityModule.GetValidAudiences());
        }

        [Fact]
        public void GivenASecurityConfigurationWithNoAudienceSpecified_WhenGettingValidAudiences_ThenNullShouldBeReturned()
        {
            var dicomServerConfiguration = new DicomServerConfiguration
            {
                Security =
                {
                    Authentication = new AuthenticationConfiguration(),
                },
            };

            var securityModule = new SecurityModule(dicomServerConfiguration);

            Assert.Null(securityModule.GetValidAudiences());
        }
    }
}
