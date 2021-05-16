# DICOM Conformance Statement

> This is currently a work-in progress document

The **Azure for Health API** supports a subset of the DICOMweb&trade; Standard. Support includes:

- [Store (STOW-RS)](#store-stow-rs)
- [Retrieve (WADO-RS)](#retrieve-wado-rs)
- [Search (QIDO-RS)](#search-qido-rs)

Additionally, the following non-standard API(s) are supported:

- [Delete](#delete)

## Store (STOW-RS)

This transaction uses the POST method to Store representations of Studies, Series, and Instances contained in the request payload.

| Method | Path               | Description |
| :----- | :----------------- | :---------- |
| POST   | ../studies         | Store instances. |
| POST   | ../studies/{study} | Store instances for a specific study. |

Parameter `study` corresponds to the DICOM attribute StudyInstanceUID. If specified, any instance that does not belong to the provided study will be rejected with `43265` warning code.

The following `Accept` header(s) for the response are supported:

- `application/dicom+json`

The following `Content-Type` header(s) are supported:

- `multipart/related; type="application/dicom"`

> Note: The Server will <u>not</u> coerce or replace attributes that conflict with existing data. All data will be stored as provided.

The following DICOM elements are required to be present in every DICOM file attempting to be stored:

- StudyInstanceUID
- SeriesInstanceUID
- SOPInstanceUID
- SOPClassUID
- PatientID

> Note: All identifiers must be between 1 and 64 characters long, and only contain alpha numeric characters or the following special characters: '.', '-'.

Each file stored must have a unique combination of StudyInstanceUID, SeriesInstanceUID and SopInstanceUID. The warning code `45070` will be returned if a file with the same identifiers already exists.

> DICOM File Size Limit: there is a size limit of 2GB for a DICOM file by default.

### Store Response Status Codes

| Code                         | Description |
| :--------------------------- | :---------- |
| 200 (OK)                     | All the SOP instances in the request have been stored. |
| 202 (Accepted)               | Some instances in the request have been stored but others have failed. |
| 204 (No Content)             | No content was provided in the store transaction request. |
| 400 (Bad Request)            | The request was badly formatted. For example, the provided study instance identifier did not conform the expected UID format. |
| 401 (Unauthorized)           | The client is not authenticated. |
| 406 (Not Acceptable)         | The specified `Accept` header is not supported. |
| 409 (Conflict)               | None of the instances in the store transaction request have been stored. |
| 415 (Unsupported Media Type) | The provided `Content-Type` is not supported. |
| 503 (Service Unavailable)    | The service is unavailable or busy. Please try again later. |

### Store Response Payload

The response payload will populate a DICOM dataset with the following elements:

| Tag          | Name                  | Description |
| :----------- | :-------------------- | :---------- |
| (0008, 1190) | RetrieveURL           | The Retrieve URL of the study if the StudyInstanceUID was provided in the store request and at least one instance is successfully stored. |
| (0008, 1198) | FailedSOPSequence     | The sequence of instances that failed to store. |
| (0008, 1199) | ReferencedSOPSequence | The sequence of stored instances. |

Each dataset in the `FailedSOPSequence` will have the following elements (if the DICOM file attempting to be stored could be read):

| Tag          | Name                     | Description |
| :----------- | :----------------------- | :---------- |
| (0008, 1150) | ReferencedSOPClassUID    | The SOP class unique identifier of the instance that failed to store. |
| (0008, 1150) | ReferencedSOPInstanceUID | The SOP instance unique identifier of the instance that failed to store. |
| (0008, 1197) | FailureReason            | The reason code why this instance failed to store. |

Each dataset in the `ReferencedSOPSequence` will have the following elements:

| Tag          | Name                     | Description |
| :----------- | :----------------------- | :---------- |
| (0008, 1150) | ReferencedSOPClassUID    | The SOP class unique identifier of the instance that failed to store. |
| (0008, 1150) | ReferencedSOPInstanceUID | The SOP instance unique identifier of the instance that failed to store. |
| (0008, 1190) | RetrieveURL              | The retrieve URL of this instance on the DICOM server. |

An example response with `Accept` header `application/dicom+json`:

```json
{
  "00081190":
  {
    "vr":"UR",
    "Value":["http://localhost/studies/d09e8215-e1e1-4c7a-8496-b4f6641ed232"]
  },
  "00081198":
  {
    "vr":"SQ",
    "Value":
    [{
      "00081150":
      {
        "vr":"UI","Value":["cd70f89a-05bc-4dab-b6b8-1f3d2fcafeec"]
      },
      "00081155":
      {
        "vr":"UI",
        "Value":["22c35d16-11ce-43fa-8f86-90ceed6cf4e7"]
      },
      "00081197":
      {
        "vr":"US",
        "Value":[43265]
      }
    }]
  },
  "00081199":
  {
    "vr":"SQ",
    "Value":
    [{
      "00081150":
      {
        "vr":"UI",
        "Value":["d246deb5-18c8-4336-a591-aeb6f8596664"]
      },
      "00081155":
      {
        "vr":"UI",
        "Value":["4a858cbb-a71f-4c01-b9b5-85f88b031365"]
      },
      "00081190":
      {
        "vr":"UR",
        "Value":["http://localhost/studies/d09e8215-e1e1-4c7a-8496-b4f6641ed232/series/8c4915f5-cc54-4e50-aa1f-9b06f6e58485/instances/4a858cbb-a71f-4c01-b9b5-85f88b031365"]
      }
    }]
  }
}
```

### Store Failure Reason Codes

| Code  | Description |
| :---- | :---------- |
| 272   | The store transaction did not store the instance because of a general failure in processing the operation. |
| 43264 | The DICOM instance failed the validation. |
| 43265 | The provided instance StudyInstanceUID did not match the specified StudyInstanceUID in the store request. |
| 45070 | A DICOM instance with the same StudyInstanceUID, SeriesInstanceUID and SopInstanceUID has already been stored. If you wish to update the contents, delete this instance first. |
| 45071 | A DICOM instance is being created by another process, or the previous attempt to create has failed and the cleanup process has not had chance to clean up yet. Please delete the instance first before attempting to create again. |

## Retrieve (WADO-RS)

This Retrieve Transaction offers support for retrieving stored studies, series, instances and frames by reference.

| Method | Path                                                                    | Description |
| :----- | :---------------------------------------------------------------------- | :---------- |
| GET    | ../studies/{study}                                                      | Retrieves all instances within a study. |
| GET    | ../studies/{study}/metadata                                             | Retrieves the metadata for all instances within a study. |
| GET    | ../studies/{study}/series/{series}                                      | Retrieves all instances within a series. |
| GET    | ../studies/{study}/series/{series}/metadata                             | Retrieves the metadata for all instances within a series. |
| GET    | ../studies/{study}/series/{series}/instances/{instance}                 | Retrieves a single instance. |
| GET    | ../studies/{study}/series/{series}/instances/{instance}/metadata        | Retrieves the metadata for a single instance. |
| GET    | ../studies/{study}/series/{series}/instances/{instance}/frames/{frames} | Retrieves one or many frames from a single instance. To specify more than one frame, a comma separate each frame to return, e.g. /studies/1/series/2/instance/3/frames/4,5,6 |

### Retrieve instances within Study or Series

The following `Accept` header(s) are supported for retrieving instances within a study or a series:


- `multipart/related; type="application/dicom"; transfer-syntax=*`
- `multipart/related; type="application/dicom";` (when transfer-syntax is not specified, 1.2.840.10008.1.2.1 is used as default)
- `multipart/related; type="application/dicom"; transfer-syntax=1.2.840.10008.1.2.1`
- `multipart/related; type="application/dicom"; transfer-syntax=1.2.840.10008.1.2.4.90`

### Retrieve an Instance

The following `Accept` header(s) are supported for retrieving a specific instance:

- `application/dicom; transfer-syntax=*`
- `multipart/related; type="application/dicom"; transfer-syntax=*`
- `application/dicom;` (when transfer-syntax is not specified, 1.2.840.10008.1.2.1 is used as default)
- `multipart/related; type="application/dicom"` (when transfer-syntax is not specified, 1.2.840.10008.1.2.1 is used as default)
- `application/dicom; transfer-syntax=1.2.840.10008.1.2.1`
- `multipart/related; type="application/dicom"; transfer-syntax=1.2.840.10008.1.2.1`
- `application/dicom; transfer-syntax=1.2.840.10008.1.2.4.90`
- `multipart/related; type="application/dicom"; transfer-syntax=1.2.840.10008.1.2.4.90`

### Retrieve Frames

The following `Accept` headers are supported for retrieving frames:
- `multipart/related; type="application/octet-stream"; transfer-syntax=*`
- `multipart/related; type="application/octet-stream";` (when transfer-syntax is not specified, 1.2.840.10008.1.2.1 is used as default)
- `multipart/related; type="application/octet-stream"; transfer-syntax=1.2.840.10008.1.2.1`
- `multipart/related; type="image/jp2";` (when transfer-syntax is not specified, 1.2.840.10008.1.2.4.90 is used as default)
- `multipart/related; type="image/jp2";transfer-syntax=1.2.840.10008.1.2.4.90`

### Retrieve Transfer Syntax

When the requested transfer syntax is different from original file, the original file is transcoded to requested transfer syntax. The original file needs to be one of below formats for transcoding to succeed, otherwise transcoding may fail:
- 1.2.840.10008.1.2 (Little Endian Implicit)
- 1.2.840.10008.1.2.1 (Little Endian Explicit)
- 1.2.840.10008.1.2.2 (Explicit VR Big Endian)
- 1.2.840.10008.1.2.4.50 (JPEG Baseline Process 1)
- 1.2.840.10008.1.2.4.57 (JPEG Lossless)
- 1.2.840.10008.1.2.4.70 (JPEG Lossless Selection Value 1)
- 1.2.840.10008.1.2.4.90 (JPEG 2000 Lossless Only)
- 1.2.840.10008.1.2.4.91 (JPEG 2000)
- 1.2.840.10008.1.2.5 (RLE Lossless)

An unsupported `transfer-syntax` will result in `406 Not Acceptable`.

### Retrieve Metadata (for Study, Series, or Instance)

The following `Accept` header(s) are supported for retrieving metadata for a study, a series, or an instance:

- `application/dicom+json`

Retrieving metadata will not return attributes with the following value representations:

| VR Name | Description            |
| :------ | :--------------------- |
| OB      | Other Byte             |
| OD      | Other Double           |
| OF      | Other Float            |
| OL      | Other Long             |
| OV      | Other 64-Bit Very Long |
| OW      | Other Word             |
| UN      | Unknown                |

### Retrieve Metadata Cache Validation (for Study, Series, or Instance)

Cache validation is supported using the `ETag` mechanism. In the response of a metadata reqeuest, ETag is returned as one of the headers. This ETag can be cached and added as `If-None-Match` header in the later requests for the same metadata. Two types of responses are possible if the data exists:
- Data has not changed since the last request: HTTP 304 (Not Modified) response will be sent with no body.
- Data has changed since the last request: HTTP 200 (OK) response will be sent with updated ETag. Required data will also be returned as part of the body.

### Retrieve Response Status Codes

| Code                         | Description |
| :--------------------------- | :---------- |
| 200 (OK)                     | All requested data has been retrieved. |
| 304 (Not Modified)           | The requested data has not modified since the last request. Content is not added to the response body in such case. Please see [Retrieve Metadata Cache Validation (for Study, Series, or Instance)](###Retrieve-Metadata-Cache-Validation-(for-Study,-Series,-or-Instance)) for more information. |
| 400 (Bad Request)            | The request was badly formatted. For example, the provided study instance identifier did not conform the expected UID format or the requested transfer-syntax encoding is not supported. |
| 401 (Unauthorized)           | The client is not authenticated. |
| 404 (Not Found)              | The specified DICOM resource could not be found. |
| 406 (Not Acceptable)         | The specified `Accept` header is not supported. |
| 503 (Service Unavailable)    | The service is unavailable or busy. Please try again later. |

## Search (QIDO-RS)

Query based on ID for DICOM Objects (QIDO) enables you to search for studies, series and instances by attributes.

| Method | Path                                            | Description                       |
| :----- | :---------------------------------------------- | :-------------------------------- |
| *Search for Studies*                                                                         |
| GET    | ../studies?...                                  | Search for studies                |
| *Search for Series*                                                                          |
| GET    | ../series?...                                   | Search for series                 |
| GET    |../studies/{study}/series?...                    | Search for series in a study      |
| *Search for Instances*                                                                       |
| GET    |../instances?...                                 | Search for instances              |
| GET    |../studies/{study}/instances?...                 | Search for instances in a study   |
| GET    |../studies/{study}/series/{series}/instances?... | Search for instances in a series  |

The following `Accept` header(s) are supported for searching:

- `application/dicom+json`

### Supported Search Parameters

The following parameters for each query are supported:

| Key              | Support Value(s)              | Allowed Count | Description |
| :--------------- | :---------------------------- | :------------ | :---------- |
| `{attributeID}=` | {value}                       | 0...N         | Search for attribute/ value matching in query. |
| `includefield=`  | `{attributeID}`<br/>`all`   | 0...N         | The additional attributes to return in the response. Both, public and private tags are supported.<br/>When `all` is provided, please see [Search Response](###Search-Response) for more information about which attributes will be returned for each query type.<br/>If a mixture of {attributeID} and 'all' is provided, the server will default to using 'all'. |
| `limit=`         | {value}                       | 0..1          | Integer value to limit the number of values returned in the response.<br/>Value can be between the range 1 >= x <= 200. Defaulted to 100. |
| `offset=`        | {value}                       | 0..1          | Skip {value} results.<br/>If an offset is provided larger than the number of search query results, a 204 (no content) response will be returned. |
| `fuzzymatching=` | `true` \| `false`             | 0..1          | If true fuzzy matching is applied to PatientName attribute. It will do a prefix word match of any name part inside PatientName value. For example, if PatientName is "John^Doe", then "joh", "do", "jo do", "Doe" and "John Doe" will all match. However "ohn" will not match. |

#### Searchable Attributes

We support searching on below attributes and search type.

| Attribute Keyword | Study | Series | Instance |
| :---------------- | :---: | :----: | :------: |
| StudyInstanceUID | X | X | X |
| PatientName | X | X | X |
| PatientID | X | X | X |
| AccessionNumber | X | X | X |
| ReferringPhysicianName | X | X | X |
| StudyDate | X | X | X |
| StudyDescription | X | X | X |
| SeriesInstanceUID |  | X | X |
| Modality |  | X | X |
| PerformedProcedureStepStartDate |  | X | X |
| SOPInstanceUID |  |  | X |

#### Search Matching

We support below matching types.

| Search Type | Supported Attribute | Example |
| :---------- | :------------------ | :------ |
| Range Query | StudyDate | {attributeID}={value1}-{value2}. For date/ time values, we supported an inclusive range on the tag. This will be mapped to `attributeID >= {value1} AND attributeID <= {value2}`. |
| Exact Match | All supported attributes | {attributeID}={value1} |
| Fuzzy Match | PatientName | Matches any component of the patient name which starts with the value. |

#### Attribute ID

Tags can be encoded in a number of ways for the query parameter. We have partially implemented the standard as defined in [PS3.18 6.7.1.1.1](http://dicom.nema.org/medical/dicom/2019a/output/chtml/part18/sect_6.7.html#sect_6.7.1.1.1). The following encodings for a tag are supported:

| Value            | Example          |
| :--------------- | :--------------- |
| {group}{element} | 0020000D         |
| {dicomKeyword}   | StudyInstanceUID |

Example query searching for instances: **../instances?Modality=CT&00280011=512&includefield=00280010&limit=5&offset=0**

### Search Response

The response will be an array of DICOM datasets. Depending on the resource, by *default* the following attributes are returned:

#### Default Study tags

| Tag          | Attribute Name |
| :----------- | :------------- |
| (0008, 0005) | SpecificCharacterSet |
| (0008, 0020) | StudyDate |
| (0008, 0030) | StudyTime |
| (0008, 0050) | AccessionNumber |
| (0008, 0056) | InstanceAvailability |
| (0009, 0090) | ReferringPhysicianName |
| (0008, 0201) | TimezoneOffsetFromUTC |
| (0010, 0010) | PatientName |
| (0010, 0020) | PatientID |
| (0010, 0030) | PatientBirthDate |
| (0010, 0040) | PatientSex |
| (0020, 0010) | StudyID |
| (0020, 000D) | StudyInstanceUID |

#### Default Series tags

| Tag          | Attribute Name |
| :----------- | :------------- |
| (0008, 0005) | SpecificCharacterSet |
| (0008, 0060) | Modality |
| (0008, 0201) | TimezoneOffsetFromUTC |
| (0008, 103E) | SeriesDescription |
| (0020, 000E) | SeriesInstanceUID |
| (0040, 0244) | PerformedProcedureStepStartDate |
| (0040, 0245) | PerformedProcedureStepStartTime |
| (0040, 0275) | RequestAttributesSequence |

#### Default Instance tags

| Tag          | Attribute Name |
| :----------- | :------------- |
| (0008, 0005) | SpecificCharacterSet |
| (0008, 0016) | SOPClassUID |
| (0008, 0018) | SOPInstanceUID |
| (0008, 0056) | InstanceAvailability |
| (0008, 0201) | TimezoneOffsetFromUTC |
| (0020, 0013) | InstanceNumber |
| (0028, 0010) | Rows |
| (0028, 0011) | Columns |
| (0028, 0100) | BitsAllocated |
| (0028, 0008) | NumberOfFrames |

If includefield=all, below attributes are included along with default attributes. Along with default attributes, this is the full list of attributes supported at each resource level.

#### Additional Study tags

| Tag          | Attribute Name |
| :----------- | :------------- |
| (0008, 1030) | Study Description |
| (0008, 0063) | AnatomicRegionsInStudyCodeSequence |
| (0008, 1032) | ProcedureCodeSequence |
| (0008, 1060) | NameOfPhysiciansReadingStudy |
| (0008, 1080) | AdmittingDiagnosesDescription |
| (0008, 1110) | ReferencedStudySequence |
| (0010, 1010) | PatientAge |
| (0010, 1020) | PatientSize |
| (0010, 1030) | PatientWeight |
| (0010, 2180) | Occupation |
| (0010, 21B0) | AdditionalPatientHistory |

#### Additional Series tags

| Tag          | Attribute Name |
| :----------- | :------------- |
| (0020, 0011) | SeriesNumber |
| (0020, 0060) | Laterality |
| (0008, 0021) | SeriesDate |
| (0008, 0031) | SeriesTime |

Along with those below attributes are returned:

- All the match query parameters and UIDs in the resource url.
- IncludeField attributes supported at that resource level. 
- If the target resource is All Series, then Study level attributes are also returned.
- If the target resource is All Instances, then Study and Series level attributes are also returned.
- If the target resource is Study's Instances, then Series level attributes are also returned.

### Search Response Codes

The query API will return one of the following status codes in the response:

| Code                      | Description |
| :------------------------ | :---------- |
| 200 (OK)                  | The response payload contains all the matching resource. |
| 204 (No Content)          | The search completed successfully but returned no results. |
| 400 (Bad Request)         | The server was unable to perform the query because the query component was invalid. Response body contains details of the failure. |
| 401 (Unauthorized)        | The client is not authenticated. |
| 503 (Service Unavailable) | The service is unavailable or busy. Please try again later. |

### Additional Notes

- Querying using the `TimezoneOffsetFromUTC` (`00080201`) is not supported.
- The query API will not return 413 (request entity too large). If the requested query response limit is outside of the acceptable range, a bad request will be returned. Anything requested within the acceptable range, will be resolved.
- When target resource is Study/Series there is a potential for inconsistent study/series level metadata across multiple instances. For example, two instances could have different patientName. In this case latest will win and you can search only on the latest data.
- Paged results are optimized to return matched *newest* instance first, this may result in duplicate records in subsequent pages if newer data matching the query was added.
- Matching is case in-sensitive and accent in-sensitive for PN VR types.
- Matching is case in-sensitive and accent sensitive for other string VR types.

## Delete

This transaction is not part of the official DICOMweb&trade; Standard. It uses the DELETE method to remove representations of Studies, Series, and Instances from the store.

| Method | Path                                                    | Description |
| :----- | :------------------------------------------------------ | :---------- |
| DELETE | ../studies/{study}                                      | Delete all instances for a specific study. |
| DELETE | ../studies/{study}/series/{series}                      | Delete all instances for a specific series within a study. |
| DELETE | ../studies/{study}/series/{series}/instances/{instance} | Delete a specific instance within a series. |

Parameters `study`, `series` and `instance` correspond to the DICOM attributes StudyInstanceUID, SeriesInstanceUID and SopInstanceUID respectively.

There are no restrictions on the request's `Accept` header, `Content-Type` header or body content.

> Note: After a Delete transaction the deleted instances will not be recoverable.

### Response Status Codes

| Code                         | Description |
| :--------------------------- | :---------- |
| 204 (No Content)             | When all the SOP instances have been deleted. |
| 400 (Bad Request)            | The request was badly formatted. |
| 401 (Unauthorized)           | The client is not authenticated. |
| 404 (Not Found)              | When the specified series was not found within a study, or the specified instance was not found within the series. |
| 503 (Service Unavailable)    | The service is unavailable or busy. Please try again later. |

### Delete Response Payload

The response body will be empty. The status code is the only useful information returned.
