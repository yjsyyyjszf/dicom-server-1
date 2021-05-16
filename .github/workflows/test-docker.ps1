#!/usr/bin/env pwsh

#region start docker
<#[SuppressMessage("Microsoft.Security", "CS002:SecretInNextLine", Justification="random test password for sql")]#>
$env:SAPASSWORD='123!@#passforCI#$' 
docker-compose  -f samples/docker/docker-compose.yaml up -d
echo 'docker up'
#endregion

#region wait for dicom-api to be heatlhy
do {
    sleep 1
    curl localhost:8080/health/check -f
} while($LASTEXITCODE -ne 0)     
echo 'dicom-api healthy'
#endregion

#region upload image to dicom
curl --location --request POST "http://localhost:8080/studies" --header "Accept: application/dicom+json" --header "Content-Type: application/dicom" --data-binary "@docs/dcms/green-square.dcm" -f --verbose
if($LASTEXITCODE -ne 0){
    exit $LASTEXITCODE
}
echo 'uploaded item to dicom-api'
#endregion

#region verify dicom-cast syncs to fhir
$success=$false
while(-not $success) {
    try{
        sleep 1
        $f = curl http://localhost:8081/Patient | convertfrom-json -ashashtable
        $success = $f.entry[0].resource.gender -eq 'female'
    }catch{}
}
echo 'item synced to dicom-cast'
#endregion
