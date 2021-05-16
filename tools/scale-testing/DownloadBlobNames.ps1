﻿$CurrentDirectory = (pwd).path

$CommonModule = -join($CurrentDirectory, '\', 'Common.psm1')
Import-Module $CommonModule -Force

$txt = '.txt'

build($RetrieveBlobNamesProject)

$instancesFileName = -join($CurrentDirectory, '\instances', $txt)
$seriesFileName = -join($CurrentDirectory, '\series', $txt)
$studiesFileName = -join($CurrentDirectory, '\studies', $txt)
Start-Process -FilePath $RetrieveBlobNamesApp -ArgumentList "$instancesFileName $seriesFileName $studiesFileName" -RedirectStandardError "log.txt"