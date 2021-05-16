﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Dicom;
using Dicom.Imaging;
using EnsureThat;
using Microsoft.Health.Dicom.Core.Exceptions;

namespace Microsoft.Health.Dicom.Core.Features.Retrieve
{
    public static class DicomFileExtensions
    {
        public static DicomPixelData GetPixelDataAndValidateFrames(this DicomFile dicomFile, IEnumerable<int> frames)
        {
            var pixelData = GetPixelData(dicomFile);
            ValidateFrames(pixelData, frames);

            return pixelData;
        }

        public static DicomPixelData GetPixelData(this DicomFile dicomFile)
        {
            EnsureArg.IsNotNull(dicomFile, nameof(dicomFile));
            DicomDataset dataset = dicomFile.Dataset;

            // Validate the dataset has the correct DICOM tags.
            if (!dataset.Contains(DicomTag.BitsAllocated) ||
                !dataset.Contains(DicomTag.Columns) ||
                !dataset.Contains(DicomTag.Rows) ||
                !dataset.Contains(DicomTag.PixelData))
            {
                throw new FrameNotFoundException();
            }

            return DicomPixelData.Create(dataset);
        }

        public static void ValidateFrames(DicomPixelData pixelData, IEnumerable<int> frames)
        {
            // Note: We look for any frame value that is less than zero, or greater than number of frames.
            var missingFrames = frames.Where(x => x >= pixelData.NumberOfFrames || x < 0).ToArray();

            // If any missing frames, throw not found exception for the specific frames not found.
            if (missingFrames.Length > 0)
            {
                throw new FrameNotFoundException();
            }
        }
    }
}
