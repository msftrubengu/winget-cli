# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

<#
    .SYNOPSIS
        Prepares WinGet PowerShell Module for local development.
    
    .PARAMETER OutDir
        The output directory where the module manifest will live.
#>

[CmdletBinding()]
param (
    [Parameter(Mandatory)]
    [string]
    $Configuration,

    [Parameter(Mandatory)]
    [string]
    $OutDir
)

$moduleName = "Microsoft.WinGet.Client"
$moduleOutDir = "$OutDir\$moduleName"
New-Item $moduleOutDir -ItemType Directory -Force -ErrorAction Stop

# Copy module files.
& "$PSScriptRoot\Copy-ModuleDirectory.ps1" -OutDir $moduleOutDir

# Copy binary module files.
$archs = 'x64','x86'
foreach ($arch in $archs)
{
    & "$PSScriptRoot\Copy-PlatformBinaries.ps1" -Platform $arch -Configuration $Configuration -OutDir $moduleOutDir -CoreFramework "net5.0-windows10.0.22000.0"
    & "$PSScriptRoot\Copy-PlatformBinaries.ps1" -Platform $arch -Configuration $Configuration -OutDir $moduleOutDir -DesktopFramework "net461"
}

# Add to module path.
if ($env:PSModulePath -notlike $OutDir) {
    $env:PSModulePath += ";$OutDir"
}

Import-Module $moduleName
