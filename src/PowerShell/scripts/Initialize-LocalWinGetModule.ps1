# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

<#
    .SYNOPSIS
        - Copies the PowerShell module output into this location.
        - Copies the modules files from the project because there's no guarantee they are updated in the module output
          location.
        - Adds the new module location to PSModulePath.
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

# Copy binaries.
Copy-Item "$PSScriptRoot\..\..\$Platform\$Configuration\PowerShell\*" "$PSScriptRoot\Module\Microsoft.WinGet.Client" -Force -Recurse -ErrorAction Stop

# Copy PowerShell files.
Copy-Item "$PSScriptRoot\..\Microsoft.WinGet.Client\Module\*" "$PSScriptRoot\Module\Microsoft.WinGet.Client" -Force -Recurse -ErrorAction Stop

# Add it to module path.
$outputModule = "$PSScriptRoot\Module"
if ($env:PSModulePath -notlike $outputModule) {
    $env:PSModulePath += ";$outputModule"
}

Import-Module Microsoft.WinGet.Client -Force

$s = @{
    source = @{
        autoUpdateIntervalInMinutes = 5
    }
}
$resource = @{
    Name = 'UserSettings'
    ModuleName = 'Microsoft.WinGet.Client'
    Property = @{
        Settings = $s
    }
}

Invoke-DscResource @resource -Method Set
