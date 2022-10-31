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

$moduleRoot = "$PSScriptRoot\Module\"

# Import-Module with Force just changes functions in the root module, not any nested ones. There's no way to load any
# updated classes. To ensure that you are running the latest version run Remove-Module
if (Get-Module -ListAvailable -Name Microsoft.WinGet.Client)
{
    Write-Host "Removing module Microsoft.WinGet.Client"
    Remove-Module Microsoft.WinGet.Client
}

# Use xcopy to copy only files that have changed.
# Copy output files from VS.
xcopy "$PSScriptRoot\..\..\$Platform\$Configuration\PowerShell\" $moduleRoot /d /s /f /y

# Copy PowerShell files. VS won't update the files if there's nothing to build.
xcopy "$PSScriptRoot\..\Microsoft.WinGet.Client\Module\" "$moduleRoot\Microsoft.WinGet.Client\" /d /s /f /y

# Add it to module path if not there.
if (-not $env:PSModulePath.Contains($moduleRoot))
{
    Write-Host "Added $moduleRoot to PSModulePath"
    $env:PSModulePath += ";$moduleRoot"
}

Write-Host "Importing module Microsoft.WinGet.Client"
Import-Module Microsoft.WinGet.Client -Force
