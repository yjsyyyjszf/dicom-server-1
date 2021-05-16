﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using MediatR;
using Microsoft.Health.Core.Features.Security.Authorization;
using Microsoft.Health.Dicom.Core.Exceptions;
using Microsoft.Health.Dicom.Core.Features.Common;
using Microsoft.Health.Dicom.Core.Features.Security;
using Microsoft.Health.Dicom.Core.Features.Validation;
using Microsoft.Health.Dicom.Core.Messages;
using Microsoft.Health.Dicom.Core.Messages.Delete;

namespace Microsoft.Health.Dicom.Core.Features.Delete
{
    public class DeleteHandler : BaseHandler, IRequestHandler<DeleteResourcesRequest, DeleteResourcesResponse>
    {
        private readonly IDeleteService _deleteService;

        public DeleteHandler(IAuthorizationService<DataActions> authorizationService, IDeleteService deleteService)
            : base(authorizationService)
        {
            _deleteService = EnsureArg.IsNotNull(deleteService, nameof(deleteService));
        }

        /// <inheritdoc />
        public async Task<DeleteResourcesResponse> Handle(DeleteResourcesRequest request, CancellationToken cancellationToken)
        {
            EnsureArg.IsNotNull(request, nameof(request));

            if (await AuthorizationService.CheckAccess(DataActions.Delete, cancellationToken) != DataActions.Delete)
            {
                throw new UnauthorizedDicomActionException(DataActions.Delete);
            }

            ValidateDeleteResourcesRequest(request);

            switch (request.ResourceType)
            {
                case ResourceType.Study:
                    await _deleteService.DeleteStudyAsync(request.StudyInstanceUid, cancellationToken);
                    break;
                case ResourceType.Series:
                    await _deleteService.DeleteSeriesAsync(request.StudyInstanceUid, request.SeriesInstanceUid, cancellationToken);
                    break;
                case ResourceType.Instance:
                    await _deleteService.DeleteInstanceAsync(request.StudyInstanceUid, request.SeriesInstanceUid, request.SopInstanceUid, cancellationToken);
                    break;
                default:
                    Debug.Fail($"Unknown delete transaction type: {request.ResourceType}", nameof(request));
                    break;
            }

            return new DeleteResourcesResponse();
        }

        private static void ValidateDeleteResourcesRequest(DeleteResourcesRequest request)
        {
            UidValidator.Validate(request.StudyInstanceUid, nameof(request.StudyInstanceUid));

            switch (request.ResourceType)
            {
                case ResourceType.Series:
                    UidValidator.Validate(request.SeriesInstanceUid, nameof(request.SeriesInstanceUid));
                    break;
                case ResourceType.Instance:
                    UidValidator.Validate(request.SeriesInstanceUid, nameof(request.SeriesInstanceUid));
                    UidValidator.Validate(request.SopInstanceUid, nameof(request.SopInstanceUid));
                    break;
            }
        }
    }
}
