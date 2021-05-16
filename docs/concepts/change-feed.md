# Change Feed Overview

The Change Feed provides logs of all the changes that occur in your Medical Imaging Server for DICOM. The Change Feed provides ordered, guaranteed, immutable, read-only log of these changes. The Change Feed offers the ability to go through the history of the Medical Imaging Server for DICOM and act upon the creates and deletes in the service.

Client applications can read these logs at any time, either in streaming or in batch mode. The Change Feed enables you to build efficient and scalable solutions that process change events that occur in your Medical Imaging Server for DICOM.

You can process these change events asynchronously, incrementally or in-full. Any number of client applications can independently read the Change Feed, in parallel, and at their own pace.

## API Design

The API exposes two `GET` endpoints for interacting with the Change Feed. A typical flow for consuming the Change Feed is [provided below](#example-usage-flow).

Verb | Route              | Returns     | Description
:--- | :----------------- | :---------- | :---
GET  | /changefeed        | Json Array  | [Read the Change Feed](#read-change-feed)
GET  | /changefeed/latest | Json Object | [Read the latest entry in the Change Feed](#get-latest-change-feed-item)

### Object model

Field               | Type      | Description
:------------------ | :-------- | :---
Sequence            | int       | The sequence id which can be used for paging (via offset) or anchoring
StudyInstanceUid    | string    | The study instance UID
SeriesInstanceUid   | string    | The series instance UID
SopInstanceUid      | string    | The sop instance UID
Action              | string    | The action that was performed - either `create` or `delete`
Timestamp           | datetime  | The date and time the action was performed in UTC
State               | string    | [The current state of the metadata](#states)
Metadata            | object    | Optionally, the current DICOM metadata if the instance exists

#### States

State    | Description
:------- | :---
current  | This instance is the current version.
replaced | This instance has been replaced by a new version.
deleted  | This instance has been deleted and is no longer available in the service.

### Read Change Feed

**Route**: /changefeed?offset={int}&limit={int}&includemetadata={**true**|false}
```
[
    {
        "Sequence": 1,
        "StudyInstanceUid": "{uid}",
        "SeriesInstanceUid": "{uid}",
        "SopInstanceUid": "{uid}",
        "Action": "create|delete",
        "Timestamp": "2020-03-04T01:03:08.4834Z",
        "State": "current|replaced|deleted",
        "Metadata": {
            "actual": "metadata"
        }
    },
    {
        "Sequence": 2,
        "StudyInstanceUid": "{uid}",
        "SeriesInstanceUid": "{uid}",
        "SopInstanceUid": "{uid}",
        "Action": "create|delete",
        "Timestamp": "2020-03-05T07:13:16.4834Z",
        "State": "current|replaced|deleted",
        "Metadata": {
            "actual": "metadata"
        }
    }
    ...
]
```

#### Parameters

Name            | Type | Description
:-------------- | :--- | :---
offset          | int  | The number of records to skip before the values to return
limit           | int  | The number of records to return (default: 10, min: 1, max: 100)
includemetadata | bool | Whether or not to include the metadata (default: true)

### Get latest Change Feed item

**Route**: /changefeed/latest?includemetadata={**true**|false}

```
{
    "Sequence": 2,
    "StudyInstanceUid": "{uid}",
    "SeriesInstanceUid": "{uid}",
    "SopInstanceUid": "{uid}",
    "Action": "create|delete",
    "Timestamp": "2020-03-05T07:13:16.4834Z",
    "State": "current|replaced|deleted",
    "Metadata": {
        "actual": "metadata"
    }
}
```

#### Parameters

Name            | Type | Description
:-------------- | :--- | :---
includemetadata | bool | Whether or not to include the metadata (default: true)

## Usage

### DICOM Cast

[DICOM Cast](/converter/dicom-cast) is a stateful processor that pulls DICOM changes from Change Feed, transforms and publishes them to a configured Azure API for FHIR service as an [ImagingStudy resource](https://www.hl7.org/fhir/imagingstudy.html). DICOM Cast can start processing the DICOM change events at any point and continue to pull and process new changes incrementally.

### Example Usage Flow

Below is the flow for an example application that wants to do additional processing on the instances within the DICOM service.

1. Application that wants to monitor the Change Feed starts.
2. It determines if there's a current state that it should start with:
   * If it has a state, it uses the offset (sequence) stored.
   * If it has never started and wants to start from beginning it uses offset=0  
   * If it only wants to process from now, it queries `/changefeed/latest` to obtain the last sequence
3. It queries the Change Feed with the given offset `/changefeed?offset={offset}`
4. If there are entries:
   * It performs additional processing  
   * It updates it's current state  
   * It starts again at 2 above  
5. If there are no entries it sleeps for a configured amount of time and starts back at 2.

### Other potential usage patterns

Change Feed support is well-suited for scenarios that process data based on objects that have changed. For example, it can be used to:

* Build connected application pipelines like ML that react to change events or schedule executions based on created or deleted instance.
* Extract business analytics insights and metrics, based on changes that occur to your objects.
* Poll the Change Feed to create an event source for push notifications.

## Summary

In this Concept, we reviewed the REST API design of Change Feed and potential usage scenarios. For a how-to guide on Change Feed, see [Pull changes from Change Feed](../how-to-guides/pull-changes-from-change-feed.md).
