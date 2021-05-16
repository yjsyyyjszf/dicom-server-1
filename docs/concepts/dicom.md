# Medical Imaging Server for DICOM Overview

## Medical Imaging

Medical imaging is the technique and process of creating visual representations of the interior of a body for clinical analysis and medical intervention, as well as visual representation of the function of some organs or tissues (physiology). Medical imaging seeks to reveal internal structures hidden by the skin and bones, as well as to diagnose and treat disease. Medical imaging also establishes a database of normal anatomy and physiology to make it possible to identify abnormalities. Although imaging of removed organs and tissues can be performed for medical reasons, such procedures are usually considered part of pathology instead of medical imaging. [Wikipedia, 2020](https://en.wikipedia.org/wiki/Medical_imaging)

## DICOM

DICOM (Digital Imaging and Communications in Medicine) is the international standard to transmit, store, retrieve, print, process, and display medical imaging information, and is the primary medical imaging standard accepted across healthcare. Although some exceptions exist (dentistry, veterinary), nearly all medical specialties, equipment manufacturers, software vendors and individual practitioners rely on DICOM at some stage of any medical workflow involving imaging. DICOM ensures that medical images meet quality standards, so that the accuracy of diagnosis can be preserved. Most imaging modalities, including CT, MRI and ultrasound must conform to the DICOM standards. Images that are in the DICOM format need to be accessed and used through specialized DICOM applications.

## Medical Imaging Server for DICOM

The Medical Imaging Server for DICOM is an open source DICOM server that is easily deployed on Azure. The Medical Imaging Server for DICOM injects DICOM metadata into the [Azure API for FHIR service](https://docs.microsoft.com/azure/healthcare-apis/), allowing a single source of truth for both clinical data and imaging metadata. It allows standards-based communication with any DICOMweb&trade; enabled systems.

The need to effectively integrate non-clinical data has become acute. In order to effectively treat patients, research new treatments or diagnostic solutions or simply provide an effective overview of the health history of a single patient, organizations must integrate data across several sources. One of the most pressing integrations is between clinical and imaging data.

FHIR&trade; is becoming an important standard for clinical data and provides extensibility to support integration of other types of data directly, or through references. By using the Medical Imaging Server for DICOM, organizations can store references to imaging data in FHIR&trade; and enable queries that cross clinical and imaging datasets. This can enable many different scenarios, for example:

- **Creating cohorts for research.** Often through queries for patients that match data in both clinical and imaging systems, such as this one (which triggered the effort to integrate FHIR&trade; and DICOM data): “Give me all the medications prescribed with all the CT Scan documents and their associated radiology reports for any patient older than 45 that has had a diagnosis of osteosarcoma over the last 2 years.”
- **Finding outcomes for similar patients to understand options and plan treatments.** When presented with a patient diagnosis, a physician can identify patient outcomes and treatment plans for past patients with a similar diagnosis, even when these include imaging data.
- **Providing a longitudinal view of a patient during diagnosis.** Radiologists, especially teleradiologists, often do not have complete access to a patient’s medical history and related imaging studies. Through FHIR&trade; integration, this data can be easily provided, even to radiologists outside of the organization’s local network.
- **Closing the feedback loop with teleradiologists.** Ideally a radiologist has access to a hospital’s clinical data to close the feedback loop after making a recommendation. However, for teleradiologists this is often not the case. Instead, they are often unable to close the feedback loop after performing a diagnosis, since they do not have access to patient data after the initial read. With no (or limited) access to clinical results or outcomes, they cannot get the feedback necessary to improve their skills. As on teleradiologist put it: “Take parathyroid for example. We do more than any other clinic in the country, and yet I have to beg and plead for surgeons to tell me what they actually found. Out of the more than 500 studies I do each month, I get direct feedback on only three or four.”  Through integration with FHIR&trade;, an organization can easily create a tool that will provide direct feedback to teleradiologists, helping them to hone their skills and make better recommendations in the future.
- **Closing the feedback loop for AI/ML models.** Machine learning models do best when real-world feedback can be used to improve their models. However, 3rd party ML model providers rarely get the feedback they need to improve their models over time. For instance, one ISV put it this way: “We us a combination of machine models and human experts to recommend a treatment plan for heart surgery. However, we only rarely get feedback from physicians on how accurate our plan was. For instance, we often recommend a stent size. We’d love to get feedback on if our prediction was correct, but the only time we hear from customers is when there’s a major issue with our recommendations.” As with feedback for teleradiologists, integration with FHIR&trade; allows organizations to create a mechanism to provide feedback to the model retraining pipeline.

## Deployment of Medical Imaging Server for DICOM To Azure

The Medical Imaging Server for DICOM needs an Azure subscription to configure and run the required components. These components are, by default, created inside of an existing or new Azure Resource Group to simplify management. Additionally, an Azure Active Directory account is required. The diagram below depicts all of the resources created within your resource group.

![resource-deployment](../images/dicom-deployment-architecture.png)

- **Azure SQL**: Indexes a subset of the Medical Imaging Server for DICOM metadata to support queries and to maintain a queryable log of changes.
- **App Service Plan**: Hosts the Medical Imaging Server for DICOM.
- **Azure Key Vault**: Stores critical security information.
- **Storage Account**: Blob Storage which persists all Medical Imaging Server for DICOM data and metadata.
- **Application Insights** (optional): Monitors performance of Medical Imaging Server for DICOM.
- **Azure Container Instance** (optional): Hosts the DICOM Cast service for Azure API for FHIR integration.
- **Azure API for FHIR** (optional): Persists the DICOM metadata alongside other clinical data.

## Summary

This Concept provided an overview of DICOM, Medical Imaging and the Medical Imaging Server for DICOM. To get started using the Medical Imaging Server:

- [Deploy Medical Imaging Server to Azure](../quickstarts/deploy-via-azure.md)
- [Deploy DICOM Cast](../quickstarts/deploy-dicom-cast.md)
- [Use the Medical Imaging Server for DICOM APIs](../tutorials/use-the-medical-imaging-server-apis.md)
