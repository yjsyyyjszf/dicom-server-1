﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Threading;
using Hl7.Fhir.Model;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.Health.DicomCast.Core.Features.Worker.FhirTransaction
{
    public interface IImagingStudySeriesPropertySynchronizer
    {
        /// <summary>
        /// Synchronizes the DICOM properties to <paramref name="series"/>.
        /// </summary>
        /// <param name="context">The transaction context.</param>
        /// <param name="series">The series component within study.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task SynchronizeAsync(FhirTransactionContext context, ImagingStudy.SeriesComponent series, CancellationToken cancellationToken);
    }
}
