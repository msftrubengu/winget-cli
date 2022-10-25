# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

<#
    .SYNOPSIS
        Simple script that takes the local winget module path, adds it to PSModulePath.
    
    .PARAMETER LocalModuleLocation
        The local directory where the Microsoft.WinGet.Client module lives.
#>

[CmdletBinding()]
param (
    [Parameter(Mandatory)]
    [string]
    $LocalModuleLocation
)

# Add it to module path.
if ($env:PSModulePath -notlike $LocalModuleLocation) {
    $env:PSModulePath += ";$LocalModuleLocation"
}

Import-Module Microsoft.WinGet.Client -Force
