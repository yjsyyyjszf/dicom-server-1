function Add-AadTestAuthEnvironment {
    <#
    .SYNOPSIS
    Adds all the required components for the test environment in AAD.
    .DESCRIPTION
    .PARAMETER TestAuthEnvironmentPath
    Path for the testauthenvironment.json file
    .PARAMETER EnvironmentName
    Environment name used for the test environment. This is used throughout for making names unique.
    .PARAMETER TenantAdminCredential
    Credentials for a tenant admin user. Needed to grant admin consent to client apps.
    #>
    param
    (
        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [string]$TestAuthEnvironmentPath,

        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [string]$EnvironmentName,

        [Parameter(Mandatory = $false)]
        [string]$EnvironmentLocation = "West US",

        [Parameter(Mandatory = $true)]
        [ValidateNotNull()]
        [pscredential]$TenantAdminCredential,

        [Parameter(Mandatory = $false )]
        [String]$WebAppSuffix = "azurewebsites.net",

        [Parameter(Mandatory = $false)]
        [string]$ResourceGroupName = $EnvironmentName,

        [parameter(Mandatory = $false)]
        [string]$KeyVaultName = "$EnvironmentName-ts"
    )

    Set-StrictMode -Version Latest

    # Get current AzureAd context
    try {
        $tenantInfo = Get-AzureADCurrentSessionInfo -ErrorAction Stop
    }
    catch {
        throw "Please log in to Azure AD with Connect-AzureAD cmdlet before proceeding"
    }

    # Get current Az context
    try {
        $azContext = Get-AzContext
    }
    catch {
        throw "Please log in to Azure RM with Login-AzAccount cmdlet before proceeding"
    }

    Write-Host "Setting up Test Authorization Environment for AAD"

    $testAuthEnvironment = Get-Content -Raw -Path $TestAuthEnvironmentPath | ConvertFrom-Json

    $keyVault = Get-AzKeyVault -VaultName $KeyVaultName

    if (!$keyVault) {
        Write-Host "Creating keyvault with the name $KeyVaultName"
        New-AzKeyVault -VaultName $KeyVaultName -ResourceGroupName $ResourceGroupName -Location $EnvironmentLocation | Out-Null
    }

    $retryCount = 0
    # Make sure key vault exists and is ready
    while (!(Get-AzKeyVault -VaultName $KeyVaultName )) {
        $retryCount += 1

        if ($retryCount -gt 7) {
            throw "Could not connect to the vault $KeyVaultName"
        }

        sleep 10
    }

    if ($azContext.Account.Type -eq "User") {
        Write-Host "Current context is user: $($azContext.Account.Id)"
        $currentObjectId = (Get-AzADUser -UserPrincipalName $azContext.Account.Id).Id
    }
    elseif ($azContext.Account.Type -eq "ServicePrincipal") {
        Write-Host "Current context is service principal: $($azContext.Account.Id)"
        $currentObjectId = (Get-AzADServicePrincipal -ServicePrincipalName $azContext.Account.Id).Id
    }
    else {
        Write-Host "Current context is account of type '$($azContext.Account.Type)' with id of '$($azContext.Account.Id)"
        throw "Running as an unsupported account type. Please use either a 'User' or 'Service Principal' to run this command"
    }

    if ($currentObjectId) {
        Write-Host "Adding permission to keyvault for $currentObjectId"
        Set-AzKeyVaultAccessPolicy -VaultName $KeyVaultName -ObjectId $currentObjectId -PermissionsToSecrets Get,List,Set
    }

    Write-Host "Ensuring API application exists"

    $dicomServiceAudience = Get-ServiceAudience -EnvironmentName $EnvironmentName -WebAppSuffix $WebAppSuffix

    $application = Get-AzureAdApplicationByIdentifierUri $dicomServiceAudience

    if (!$application) {
        New-DicomServerApiApplicationRegistration -DicomServiceAudience $dicomServiceAudience

        # Change to use applicationId returned
        $application = Get-AzureAdApplicationByIdentifierUri $dicomServiceAudience
    }

    Write-Host "Setting roles on API Application"
    $appRoles = ($testAuthEnvironment.users.roles + $testAuthEnvironment.clientApplications.roles) | Select-Object -Unique
    Set-DicomServerApiApplicationRoles -ApiAppId $application.AppId -AppRoles $appRoles | Out-Null

    Write-Host "Ensuring users and rol assignments for API Application exist"
    $environmentUsers = Set-DicomServerApiUsers -UserNamePrefix $EnvironmentName -TenantDomain $tenantInfo.TenantDomain -ApiAppId $application.AppId -UserConfiguration $testAuthEnvironment.Users -KeyVaultName $KeyVaultName

    $environmentClientApplications = @()

    Write-Host "Ensuring client application exists"
    foreach ($clientApp in $testAuthEnvironment.clientApplications) {
        $displayName = Get-ApplicationDisplayName -EnvironmentName $EnvironmentName -AppId $clientApp.Id
        $aadClientApplication = Get-AzureAdApplicationByDisplayName $displayName

        if (!$aadClientApplication) {

            $aadClientApplication = New-DicomServerClientApplicationRegistration -ApiAppId $application.AppId -DisplayName "$displayName" -PublicClient:$true

            $secretSecureString = ConvertTo-SecureString $aadClientApplication.AppSecret -AsPlainText -Force

        }
        else {
            $existingPassword = Get-AzureADApplicationPasswordCredential -ObjectId $aadClientApplication.ObjectId | Remove-AzureADApplicationPasswordCredential -ObjectId $aadClientApplication.ObjectId
            $newPassword = New-AzureADApplicationPasswordCredential -ObjectId $aadClientApplication.ObjectId

            $secretSecureString = ConvertTo-SecureString $newPassword.Value -AsPlainText -Force
        }

        Grant-ClientAppAdminConsent -AppId $aadClientApplication.AppId -TenantAdminCredential $TenantAdminCredential

        $environmentClientApplications += @{
            id          = $clientApp.Id
            displayName = $displayName
            appId       = $aadClientApplication.AppId
        }

        $appIdSecureString = ConvertTo-SecureString -String $aadClientApplication.AppId -AsPlainText -Force
        Set-AzKeyVaultSecret -VaultName $KeyVaultName -Name "app--$($clientApp.Id)--id" -SecretValue $appIdSecureString | Out-Null
        Set-AzKeyVaultSecret -VaultName $KeyVaultName -Name "app--$($clientApp.Id)--secret" -SecretValue $secretSecureString | Out-Null
        
        Set-DicomServerClientAppRoleAssignments -ApiAppId $application.AppId -AppId $aadClientApplication.AppId -AppRoles $clientApp.roles | Out-Null
    }

    Write-Host "Set token and auth url in key vault"
    $aadEndpoint = (Get-AzureADCurrentSessionInfo).Environment.Endpoints["ActiveDirectory"]
    $aadTenantId = (Get-AzureADCurrentSessionInfo).Tenant.Id.ToString()
    $tokenUrl  = "$aadEndpoint$aadTenantId/oauth2/token"
    $tokenUrlSecureString = ConvertTo-SecureString -String $tokenUrl -AsPlainText -Force
        
    Set-AzKeyVaultSecret -VaultName $KeyVaultName -Name "security--tokenUrl" -SecretValue $tokenUrlSecureString | Out-Null
    
    @{
        keyVaultName                  = $KeyVaultName
        environmentUsers              = $environmentUsers
        environmentClientApplications = $environmentClientApplications
    }
}
