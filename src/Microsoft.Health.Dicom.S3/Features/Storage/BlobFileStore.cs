// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using EnsureThat;
using Microsoft.Health.Dicom.Core.Features.Common;
using Microsoft.Health.Dicom.Core.Features.Model;

namespace Microsoft.Health.Dicom.S3.Features.Storage
{
    // TODO: Don't forget to make the non-implemented methods run as async after implementing.

    public class BlobFileStore : IFileStore
    {
        private const string BucketName = "test-server-v2";

        private readonly IAmazonS3 _s3Client;

        public BlobFileStore(IAmazonS3 s3Client)
        {
            _s3Client = s3Client;
        }

        public Task<Uri> StoreFileAsync(VersionedInstanceIdentifier versionedInstanceIdentifier, Stream stream,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<Stream> GetFileAsync(VersionedInstanceIdentifier versionedInstanceIdentifier,
            CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNull(versionedInstanceIdentifier, nameof(versionedInstanceIdentifier));

            string fileName = "IM-0001-0022.dcm";
            try
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = BucketName,
                    Key =
                        $"{versionedInstanceIdentifier.StudyInstanceUid}/{versionedInstanceIdentifier.SeriesInstanceUid}/{versionedInstanceIdentifier.SopInstanceUid}/{fileName}"
                };

                await _s3Client.GetObjectAsync(request, cancellationToken);
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine($"Error encountered ***. Message:'{e.Message}' when reading object");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unknown encountered on server ***. Message:'{e.Message}' when reading object");
            }
            throw new NotImplementedException();
        }

        public Task DeleteFileIfExistsAsync(VersionedInstanceIdentifier versionedInstanceIdentifier,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
