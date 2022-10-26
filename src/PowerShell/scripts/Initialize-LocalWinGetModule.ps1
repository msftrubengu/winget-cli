# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

<#
    .SYNOPSIS
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

# Add it to module path.
$outputModule = "$PSScriptRoot\Module"
if ($env:PSModulePath -notlike $outputModule) {
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
#$resource = @{
#    Name = 'UserSettings'
#    ModuleName = 'Microsoft.WinGet.Client'
#    Property = @{
#        Settings = $s
#    }
#}
#
#Invoke-DscResource @resource -Method Set
