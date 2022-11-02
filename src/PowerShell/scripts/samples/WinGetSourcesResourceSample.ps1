# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

<#
    .SYNOPSIS
        Simple sample on how to use WinGetSourcesResource DSC resource.
        Requires PowerShell DSC 3.0 https://learn.microsoft.com/en-us/powershell/dsc/overview?view=dsc-3.0
        IMPORTANT: This deletes the main winget source and add it again.
#>

#Requires -Modules Microsoft.WinGet.Client, Microsoft.WinGet.DSC

using module Microsoft.WinGet.DSC
using namespace System.Collections.Generic

$resource = @{
    Name = 'WinGetSourcesResource'
    ModuleName = 'Microsoft.WinGet.DSC'
    Property = @{
    }
}

$getResult = Invoke-DscResource @resource -Method Get
Write-Host "Current sources"

# TODO: figure out why this gets printed until the end
#$getResult.Sources

foreach ($source in $getResult.Sources)
{
    Write-Host "Name $($source.Name) Argument $($source.Argument) Type $($source.Type)"
}

$expectedSources = [List[WinGetSource]]::new()
$expectedSources.Add([WinGetSource]::new(
    "winget",
    "https://cdn.winget.microsoft.com/cache"))

$resource.Property = @{
    Sources = $expectedSources
}

# The default value comparison for test is Partial, so if you have the winget source this should succeed.
$testResult = Invoke-DscResource @resource -Method Test
if ($testResult.InDesiredState)
{
    Write-Host "winget source is present"
}

# A full match will fail if there are more sources.
$resource.Property = @{
    Sources = $expectedSources
    Action = [WinGetAction]::Full
}
$testResult = Invoke-DscResource @resource -Method Test
if (-not $testResult.InDesiredState)
{
    Write-Host "winget source is not the only source"
}
else
{
    Write-Host "winget source is the only source"
}

# Breaking winget. Note this will fail if not run as admin.
$resource.Property = @{
    Sources = $expectedSources
    Action = [WinGetAction]::Partial
    Command = [SourceCommand]::Remove
}
Invoke-DscResource @resource -Method Set | Out-Null

# Test again
$testResult = Invoke-DscResource @resource -Method Test
if (-not $testResult.InDesiredState)
{
    Write-Host "winget source is gone forever"
}

# nvm, add it again
$resource.Property.Command = [SourceCommand]::Add
Invoke-DscResource @resource -Method Set | Out-Null
