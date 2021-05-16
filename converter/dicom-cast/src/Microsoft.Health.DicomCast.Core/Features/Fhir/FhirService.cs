﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Hl7.Fhir.Model;
using Microsoft.Health.DicomCast.Core.Extensions;
using Microsoft.Health.Fhir.Client;
using static Hl7.Fhir.Model.CapabilityStatement;
using IFhirClient = Microsoft.Health.Fhir.Client.IFhirClient;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.Health.DicomCast.Core.Features.Fhir
{
    /// <summary>
    /// Provides functionalities to communicate with FHIR server.
    /// </summary>
    public class FhirService : IFhirService
    {
        private readonly IFhirClient _fhirClient;
        private readonly IFhirResourceValidator _fhirResourceValidator;

        private readonly IEnumerable<FHIRVersion> _supportedFHIRVersions = new List<FHIRVersion> { FHIRVersion.N4_0_0, FHIRVersion.N4_0_1 };

        public FhirService(IFhirClient fhirClient, IFhirResourceValidator fhirResourceValidator)
        {
            EnsureArg.IsNotNull(fhirClient, nameof(fhirClient));
            EnsureArg.IsNotNull(fhirResourceValidator, nameof(fhirResourceValidator));

            _fhirClient = fhirClient;
            _fhirResourceValidator = fhirResourceValidator;
        }

        /// <inheritdoc/>
        public Task<Patient> RetrievePatientAsync(Identifier identifier, CancellationToken cancellationToken)
            => SearchByIdentifierAsync<Patient>(identifier, cancellationToken);

        /// <inheritdoc/>
        public Task<ImagingStudy> RetrieveImagingStudyAsync(Identifier identifier, CancellationToken cancellationToken)
            => SearchByIdentifierAsync<ImagingStudy>(identifier, cancellationToken);

        /// <inheritdoc/>
        public Task<Endpoint> RetrieveEndpointAsync(string queryParameter, CancellationToken cancellationToken)
            => SearchByQueryParameterAsync<Endpoint>(queryParameter, cancellationToken);

        /// <inheritdoc/>
        public async Task CheckFhirServiceCapability(CancellationToken cancellationToken)
        {
            using FhirResponse<CapabilityStatement> response = await _fhirClient.ReadAsync<CapabilityStatement>("metadata", cancellationToken);
            FHIRVersion version = response.Resource.FhirVersion ?? throw new InvalidFhirServerException(DicomCastCoreResource.FailedToValidateFhirVersion);
            if (!_supportedFHIRVersions.Contains(version))
            {
                throw new InvalidFhirServerException(DicomCastCoreResource.InvalidFhirServerVersion);
            }

            foreach (RestComponent element in response.Resource.Rest)
            {
                foreach (SystemInteractionComponent interaction in element.Interaction)
                {
                    if (interaction.Code == SystemRestfulInteraction.Transaction)
                    {
                        return;
                    }
                }
            }

            throw new InvalidFhirServerException(DicomCastCoreResource.FhirServerTransactionNotSupported);
        }

        private async Task<TResource> SearchByIdentifierAsync<TResource>(Identifier identifier, CancellationToken cancellationToken)
            where TResource : Resource, new()
        {
            EnsureArg.IsNotNull(identifier, nameof(identifier));

            return await SearchByQueryParameterAsync<TResource>(identifier.ToSearchQueryParameter(), cancellationToken);
        }

        private async Task<TResource> SearchByQueryParameterAsync<TResource>(string queryParameter, CancellationToken cancellationToken)
            where TResource : Resource, new()
        {
            EnsureArg.IsNotNullOrEmpty(queryParameter, nameof(queryParameter));

            string fhirTypeName = ModelInfo.GetFhirTypeNameForType(typeof(TResource));
            if (!Enum.TryParse(fhirTypeName, out ResourceType resourceType))
            {
                Debug.Assert(false, "Resource type could not be parsed from TResource");
            }

            Bundle bundle = await _fhirClient.SearchAsync(
                resourceType,
                queryParameter,
                count: null,
                cancellationToken);

            int matchCount = 0;
            TResource result = null;

            while (bundle != null)
            {
                matchCount += bundle.Entry.Count;

                if (matchCount > 1)
                {
                    // Multiple matches.
                    throw new MultipleMatchingResourcesException(typeof(TResource).Name);
                }
                else if (bundle.Entry.Count == 1)
                {
                    // There was only one match but because the server could return empty continuation token
                    // with more results, we need to follow the links to make sure there are no additional matching resources.
                    result = (TResource)bundle.Entry[0].Resource;
                }

                if (bundle.NextLink != null)
                {
                    bundle = await _fhirClient.SearchAsync(bundle.NextLink.ToString(), cancellationToken);
                }
                else
                {
                    break;
                }
            }

            // Validate to make sure the resource is valid.
            if (result != null)
            {
                _fhirResourceValidator.Validate(result);
            }

            return result;
        }
    }
}
