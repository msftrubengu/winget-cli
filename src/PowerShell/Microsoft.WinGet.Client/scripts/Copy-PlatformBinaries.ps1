# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

<#
    .SYNOPSIS
        Copies built binaries for a specific platform to an output directory.
    
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
    $Platform,

    [Parameter(Mandatory)]
    [string]
    $Configuration,

    [Parameter(Mandatory, ParameterSetName = 'Core')]
    [string]
    $CoreFramework,

    [Parameter(Mandatory, ParameterSetName = 'Desktop')]
    [string]
    $DesktopFramework,

    [Parameter(Mandatory)]
    [string]
    $OutDir
)

switch ($PSCmdlet.ParameterSetName) {
    'Core' {
        $frameworkFolderName = 'Core'
        $framework = $CoreFramework
        break
    }
    'Desktop' {
        $frameworkFolderName = 'Desktop'
        $framework = $DesktopFramework
        break
    }
}

$ProjectName = 'Microsoft.WinGet.Client'
Copy-Item "$PSScriptRoot\..\..\..\$Platform\$Configuration\$ProjectName\$framework" "$OutDir\$Platform\$frameworkFolderName" -Force -Recurse -ErrorAction Stop

Write-Host 'Done!' -ForegroundColor Green
