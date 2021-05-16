﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Health.Dicom.Api.Features.Routing;
using Microsoft.Health.Dicom.Core.Features.Model;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Dicom.Api.UnitTests.Features.Routing
{
    public class UrlResolverTests
    {
        private const string DefaultScheme = "http";
        private const string DefaultHost = "test";
        private const string StudyInstanceUidActionParameterName = "studyInstanceUid";
        private const string SeriesInstanceUidActionParameterName = "seriesInstanceUid";
        private const string SopInstanceUidActionParameterName = "sopInstanceUid";

        private readonly IUrlHelperFactory _urlHelperFactory = Substitute.For<IUrlHelperFactory>();
        private readonly IHttpContextAccessor _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        private readonly IActionContextAccessor _actionContextAccessor = Substitute.For<IActionContextAccessor>();

        private readonly UrlResolver _urlResolver;

        private readonly IUrlHelper _urlHelper = Substitute.For<IUrlHelper>();
        private readonly DefaultHttpContext _httpContext = new DefaultHttpContext();
        private readonly ActionContext _actionContext = new ActionContext();

        private UrlRouteContext _capturedUrlRouteContext;

        public UrlResolverTests()
        {
            _httpContext.Request.Scheme = DefaultScheme;
            _httpContext.Request.Host = new HostString(DefaultHost);

            _httpContextAccessor.HttpContext.Returns(_httpContext);

            _actionContextAccessor.ActionContext.Returns(_actionContext);

            _urlHelper.RouteUrl(
                Arg.Do<UrlRouteContext>(c => _capturedUrlRouteContext = c));

            _urlHelperFactory.GetUrlHelper(_actionContext).Returns(_urlHelper);

            _urlHelper.RouteUrl(Arg.Any<UrlRouteContext>()).Returns($"{DefaultScheme}://{DefaultHost}");

            _urlResolver = new UrlResolver(
                   _urlHelperFactory,
                   _httpContextAccessor,
                   _actionContextAccessor);
        }

        [Fact]
        public void GivenAStudy_WhenRetrieveStudyUriIsResolved_ThenCorrectUrlShouldBeReturned()
        {
            const string studyInstanceUid = "123.123";

            _urlResolver.ResolveRetrieveStudyUri(studyInstanceUid);

            ValidateUrlRouteContext(
                "RetrieveStudy",
                routeValues =>
                {
                    Assert.Equal(studyInstanceUid, routeValues[StudyInstanceUidActionParameterName]);
                });
        }

        [Fact]
        public void GivenAnInstance_WhenRetrieveInstanceUriIsResolved_ThenCorrectUrlShouldBeReturned()
        {
            const string studyInstanceUid = "123.123";
            const string seriesInstanceUid = "456.456";
            const string sopInstanceUid = "789.789";

            var instance = new InstanceIdentifier(studyInstanceUid, seriesInstanceUid, sopInstanceUid);

            _urlResolver.ResolveRetrieveInstanceUri(instance);

            ValidateUrlRouteContext(
                "RetrieveInstance",
                routeValues =>
                {
                    Assert.Equal(studyInstanceUid, routeValues[StudyInstanceUidActionParameterName]);
                    Assert.Equal(seriesInstanceUid, routeValues[SeriesInstanceUidActionParameterName]);
                    Assert.Equal(sopInstanceUid, routeValues[SopInstanceUidActionParameterName]);
                });
        }

        private void ValidateUrlRouteContext(string routeName, Action<RouteValueDictionary> routeValuesValidator = null)
        {
            Assert.NotNull(_capturedUrlRouteContext);

            Assert.Equal(routeName, _capturedUrlRouteContext.RouteName);
            Assert.Equal(DefaultScheme, _capturedUrlRouteContext.Protocol);
            Assert.Equal(DefaultHost, _capturedUrlRouteContext.Host);

            RouteValueDictionary routeValues = Assert.IsType<RouteValueDictionary>(_capturedUrlRouteContext.Values);

            routeValuesValidator(routeValues);
        }
    }
}
