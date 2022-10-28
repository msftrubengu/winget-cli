# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

<#
    .SYNOPSIS
        Helper script to setup the module locally.
        - Copies the PowerShell module output into this location.
        - Copies the modules files from the project because there's no guarantee they are updated in the module output
          location.
        - Adds the module location to PSModulePath if not there.
        - Import Microsoft.WinGet.Client module.
    
    .PARAMETER Platform
        The platform we are building for.
    
    .PARAMETER Configuration
        The configuration we are building in.
#>

[CmdletBinding()]
param (
    [Parameter(Mandatory)]
    [string]
    $Platform,

    [Parameter(Mandatory)]
    [string]
    $Configuration
)

# Use xcopy to copy only files that have changed. This allows to just upload the module files, force the import and
# keep using the same PowerShell session.

# Copy binaries.
xcopy "$PSScriptRoot\..\..\$Platform\$Configuration\PowerShell\" "$PSScriptRoot\Module\" /d /s /f /y

# Copy PowerShell files. VS won't update the files if there's nothing to build.
xcopy "$PSScriptRoot\..\Microsoft.WinGet.Client\Module\" "$PSScriptRoot\Module\Microsoft.WinGet.Client\" /d /s /f /y

# Import-Module with Force just changes functions in the root module, not any nested ones. There's no way to load any
# updated classes. To ensure that you are running the latest version run Remove-Module
if (Get-Module -ListAvailable -Name Microsoft.WinGet.Client)
{
    Remove-Module Microsoft.WinGet.Client
}

# Add it to module path if not there.
$outputModule = "$PSScriptRoot\Module"
if ($env:PSModulePath -notlike $outputModule)
{
    $env:PSModulePath += ";$outputModule"
}

Import-Module Microsoft.WinGet.Client -Force

# Example of usage:
#$s = @{
#    source = @{
#        autoUpdateIntervalInMinutes = 25
#    }
#}
#

#$newsources = @([WinGetSource]::new("int", "https://winget-int.azureedge.net/cache"))
#
#$resource = @{
#    Name = 'SourcesResource'
#    ModuleName = 'Microsoft.WinGet.Client'
#    Property = @{
#        Sources = $newsources
#    }
#}
#
#Invoke-DscResource @resource -Method Set
