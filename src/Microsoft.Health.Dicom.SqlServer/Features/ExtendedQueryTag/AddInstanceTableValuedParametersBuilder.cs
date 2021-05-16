﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Dicom;
using EnsureThat;
using Microsoft.Health.Dicom.Core.Extensions;
using Microsoft.Health.Dicom.Core.Features.ExtendedQueryTag;
using Microsoft.Health.Dicom.SqlServer.Features.Schema.Model;

namespace Microsoft.Health.Dicom.SqlServer.Features.ExtendedQueryTag
{
    /// <summary>
    /// Build AddInstanceTableValuedParameters
    /// </summary>
    internal static class AddInstanceTableValuedParametersBuilder
    {
        private static readonly Dictionary<DicomVR, Func<DicomDataset, DicomTag, DicomVR, DateTime?>> DataTimeReaders = new Dictionary<DicomVR, Func<DicomDataset, DicomTag, DicomVR, DateTime?>>()
        {
            { DicomVR.DA, Core.Extensions.DicomDatasetExtensions.GetStringDateAsDate },
        };

        /// <summary>
        /// Read Index Tag values from DicomDataset.
        /// </summary>
        /// <param name="instance">The dicom dataset.</param>
        /// <param name="queryTags">The index tags.</param>
        public static VLatest.AddInstanceTableValuedParameters Build(
            DicomDataset instance,
            IEnumerable<QueryTag> queryTags)
        {
            EnsureArg.IsNotNull(instance, nameof(instance));
            EnsureArg.IsNotNull(queryTags, nameof(queryTags));

            List<InsertStringExtendedQueryTagTableTypeV1Row> stringRows = new List<InsertStringExtendedQueryTagTableTypeV1Row>();
            List<InsertLongExtendedQueryTagTableTypeV1Row> longRows = new List<InsertLongExtendedQueryTagTableTypeV1Row>();
            List<InsertDoubleExtendedQueryTagTableTypeV1Row> doubleRows = new List<InsertDoubleExtendedQueryTagTableTypeV1Row>();
            List<InsertDateTimeExtendedQueryTagTableTypeV1Row> dateTimeRows = new List<InsertDateTimeExtendedQueryTagTableTypeV1Row>();
            List<InsertPersonNameExtendedQueryTagTableTypeV1Row> personNamRows = new List<InsertPersonNameExtendedQueryTagTableTypeV1Row>();

            foreach (var queryTag in queryTags)
            {
                ExtendedQueryTagDataType dataType = ExtendedQueryTagLimit.ExtendedQueryTagVRAndDataTypeMapping[queryTag.VR.Code];
                switch (dataType)
                {
                    case ExtendedQueryTagDataType.StringData:
                        AddStringRow(instance, stringRows, queryTag);

                        break;

                    case ExtendedQueryTagDataType.LongData:
                        AddLongRow(instance, longRows, queryTag);

                        break;

                    case ExtendedQueryTagDataType.DoubleData:
                        AddDoubleRow(instance, doubleRows, queryTag);

                        break;

                    case ExtendedQueryTagDataType.DateTimeData:
                        AddDateTimeRow(instance, dateTimeRows, queryTag);

                        break;

                    case ExtendedQueryTagDataType.PersonNameData:
                        AddPersonNameRow(instance, personNamRows, queryTag);

                        break;

                    default:
                        Debug.Fail($"Not able to handle {dataType}");
                        break;
                }
            }

            return new VLatest.AddInstanceTableValuedParameters(stringRows, longRows, doubleRows, dateTimeRows, personNamRows);
        }

        private static void AddPersonNameRow(DicomDataset instance, List<InsertPersonNameExtendedQueryTagTableTypeV1Row> personNamRows, QueryTag queryTag)
        {
            string personNameVal = instance.GetSingleValueOrDefault<string>(queryTag.Tag, expectedVR: queryTag.VR);
            if (personNameVal != null)
            {
                personNamRows.Add(new InsertPersonNameExtendedQueryTagTableTypeV1Row(queryTag.ExtendedQueryTagStoreEntry.Key, personNameVal, (byte)queryTag.Level));
            }
        }

        private static void AddDateTimeRow(DicomDataset instance, List<InsertDateTimeExtendedQueryTagTableTypeV1Row> dateTimeRows, QueryTag queryTag)
        {
            DateTime? dateVal = DataTimeReaders.TryGetValue(
                             queryTag.VR,
                             out Func<DicomDataset, DicomTag, DicomVR, DateTime?> reader) ? reader.Invoke(instance, queryTag.Tag, queryTag.VR) : null;

            if (dateVal.HasValue)
            {
                dateTimeRows.Add(new InsertDateTimeExtendedQueryTagTableTypeV1Row(queryTag.ExtendedQueryTagStoreEntry.Key, dateVal.Value, (byte)queryTag.Level));
            }
        }

        private static void AddDoubleRow(DicomDataset instance, List<InsertDoubleExtendedQueryTagTableTypeV1Row> doubleRows, QueryTag queryTag)
        {
            double? doubleVal = instance.GetSingleValueOrDefault<double>(queryTag.Tag, expectedVR: queryTag.VR);
            if (doubleVal.HasValue)
            {
                doubleRows.Add(new InsertDoubleExtendedQueryTagTableTypeV1Row(queryTag.ExtendedQueryTagStoreEntry.Key, doubleVal.Value, (byte)queryTag.Level));
            }
        }

        private static void AddLongRow(DicomDataset instance, List<InsertLongExtendedQueryTagTableTypeV1Row> longRows, QueryTag queryTag)
        {
            long? longVal = instance.GetSingleValueOrDefault<long>(queryTag.Tag, expectedVR: queryTag.VR);

            if (longVal.HasValue)
            {
                longRows.Add(new InsertLongExtendedQueryTagTableTypeV1Row(queryTag.ExtendedQueryTagStoreEntry.Key, longVal.Value, (byte)queryTag.Level));
            }
        }

        private static void AddStringRow(DicomDataset instance, List<InsertStringExtendedQueryTagTableTypeV1Row> stringRows, QueryTag queryTag)
        {
            string stringVal = instance.GetSingleValueOrDefault<string>(queryTag.Tag, expectedVR: queryTag.VR);
            if (stringVal != null)
            {
                stringRows.Add(new InsertStringExtendedQueryTagTableTypeV1Row(queryTag.ExtendedQueryTagStoreEntry.Key, stringVal, (byte)queryTag.Level));
            }
        }
    }
}
