// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Dicom;
using EnsureThat;
using Microsoft.Health.Dicom.Core.Extensions;

namespace Microsoft.Health.Dicom.S3.Models
{
    public class DataStore
    {
        public string StudyInstanceUID { get; set; }
        public string SeriesInstanceUID { get; set; }
        public string SOPInstanceUID { get; set; }
        public string PatientID { get; set; }
        public string PatientName { get; set; }
        public string ReferringPhysicianName { get; set; }
        public DateTime? StudyDate { get; set; }
        public string StudyDescription { get; set; }
        public string AccessionNumber { get; set; }
        public string Modality { get; set; }
        public DateTime? PerformedProcedureStepStartDate { get; set; }

        public DataStore(DicomDataset instance)
        {
            EnsureArg.IsNotNull(instance, nameof(instance));

            StudyInstanceUID = instance.GetString(DicomTag.StudyInstanceUID);
            SeriesInstanceUID = instance.GetString(DicomTag.SeriesInstanceUID);
            SOPInstanceUID = instance.GetString(DicomTag.SOPInstanceUID);
            PatientID = instance.GetSingleValueOrDefault<string>(DicomTag.PatientID);
            PatientName = instance.GetSingleValueOrDefault<string>(DicomTag.PatientName);
            ReferringPhysicianName = instance.GetSingleValueOrDefault<string>(DicomTag.ReferringPhysicianName);
            StudyDate = instance.GetStringDateAsDate(DicomTag.StudyDate);
            StudyDescription = instance.GetSingleValueOrDefault<string>(DicomTag.StudyDescription);
            AccessionNumber = instance.GetSingleValueOrDefault<string>(DicomTag.AccessionNumber);
            Modality = instance.GetSingleValueOrDefault<string>(DicomTag.Modality);
            PerformedProcedureStepStartDate = instance.GetStringDateAsDate(DicomTag.PerformedProcedureStepStartDate);
        }
    }
}
