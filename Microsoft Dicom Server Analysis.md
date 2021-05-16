Microsoft Dicom Server Analysis

Pros:
  * The code base has done an excelent job of seperating concerns between the different areas of the code and did an excelent job of makeing most of the components modular. Due 
  to this, we could easilly remove the modules that are used to integrate with SQL Server and Azure and replace them with our our own that worked with S3 and our infrastructure.
  * All of the dependency injection is done through Interfaces, all of which reside in the Core project. This means that all we would need to do is create our own module for
  integrating with S3 and ultralinq and all we would need to do is implement their interfaces. 

What To Remove:
  * Any uses of Microsoft Sql Server
    * Microsoft.Health.Dicom.SqlServer project entierly because it is only used to support the Extended Query Tags which we aren't currently using. A new solution using 
    different database can always be aded later with little hassle.

Additions Needed:
  * S3 Configuration Settings
  * S3 Authentication

Parts To Be Replaced
  * Any uses of Azure
    * Microsoft.Health.Dicom.Blob project entierly since we want to upload our dicom files to S3 instead. I believe that the AWSSDK.S3 NuGet package may have the functionality 
    to facilitate this.
    * Microsoft.Health.Dicom.Metadata project entirely and I think this can just be rolled into our S3 module or kept seperate depending on what we want.
  