# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

<#
    .SYNOPSIS
        Exports crescendo module and merge module manifests.
    
    .PARAMETER Platform
        The platform we are building for.
    
    .PARAMETER Configuration
        The configuration we are building in.

    .PARAMETER OutDir
        The base output directory where the module manifest will be.
#>

[CmdletBinding()]
param (
    [Parameter(Mandatory)]
    [string]
    $ConfigurationFile,

    [Parameter(Mandatory)]
    [string]
    $ModuleName,

    [Parameter(Mandatory)]
    [string]
    $ModuleOutputDirectory
)

if (-not (Get-Module Microsoft.PowerShell.Crescendo))
{
    Install-Module Microsoft.PowerShell.Crescendo -Force
}

# In a perfect world we would check if $ModuleOutputDirectory\$ModuleName.psd1 exists and if it does then load the data
# via Import-PowerShellDataFile and make sure FunctionsToExport contains all the exported functions from the generated
# psd1 file of the Export-CrescendoModule command. We have dynamic expressions on ..\Module\Microsoft.WinGet.Client.psm1
# so that can't happen easily, so we will just nicely remind you :(
$dir = $pwd
Set-Location $PSScriptRoot
Write-Host "Generating crescendo module"
Export-CrescendoModule -ConfigurationFile $ConfigurationFile -ModuleName $ModuleName -Force
Set-Location $dir

Copy-Item "$PSScriptRoot\$ModuleName.psm1" "$ModuleOutputDirectory\Public\CrescendoGeneratedFunctions.ps1" -Force -ErrorAction Stop

# In a perfect world we would check if $ModuleOutputDirectory\$ModuleName.psd1 exists and if it does then load the data
# via Import-PowerShellDataFile and make sure FunctionsToExport contains all the exported functions from the generated
# psd1 file of the Export-CrescendoModule command. We have dynamic expressions on ..\Module\Microsoft.WinGet.Client.psm1
# so that can't happen easily, so we will just nicely remind you :(
$config = Import-PowerShellDataFile -Path "$PSScriptRoot\$ModuleName.psd1"

Write-Host "Crescendo module generated. Please verify the FunctionsToExport is updated in ..\Module\Microsoft.WinGet.Client.psd1 if needed"
Write-Host "Generated FunctionsToExport $($config.FunctionsToExport)"
