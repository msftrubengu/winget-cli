# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

using namespace System.Collections.Generic

try
{
    # Load all non-test .ps1 files in the script's directory.
    Get-ChildItem -Path $PSScriptRoot\* -Filter *.ps1 -Exclude *.Tests.ps1 -Recurse | ForEach-Object { Import-Module $_.FullName }
} catch
{
    $e = $_.Exception
    while ($e.InnerException)
    {
        $e = $e.InnerException
    }

    if (-not [string]::IsNullOrWhiteSpace($e.Message))
    {
        Write-Host $e.Message -ForegroundColor Red -BackgroundColor Black
    }
}

#region classes

# Literally the same as Microsoft.Management.Deployment.PackageCatalogReference.
# Using that type in a DSC resource it fails with MethodInvocationException: Exception calling
# "ImportClassResourcesFromModule" with "4" argument(s): "The DSC resource 'PackageCatalogReference' has no default constructor."
# TODO: figure that out.
class WinGetSource
{
    [string]$Name;
    [string]$Argument;
    [string]$Type = "Microsoft.PreIndexed.Package";

    # We explicity need the default constructor
    WinGetSource()
    {
    }

    WinGetSource([string]$n)
    {
        $this.Name = $n;
    }

    WinGetSource([string]$n, [string]$a)
    {
        $this.Name = $n;
        $this.Argument = $a;
    }

    WinGetSource([string]$n, [string]$a, [string]$t)
    {
        $this.Name = $n;
        $this.Argument = $a;
        $this.Type = $t;
    }
}

#endregion classes

#region enums
enum WinGetAction
{
    Partial
    Full
}

enum SourceCommand
{
    Add
    Remove
}

#endregion enums

#region DscResources
# DSC Powershell doesn't support binary DSC resources without the MOF schema. Author here all DSC Resources.

# This resource is in charge of editing the settings.json file of winget.
[DSCResource()]
class WinGetUserSettingsResource
{
    # We need a key. Do not set.
    [DscProperty(Key)]
    [string]$SID

    # A hash table with the desired settings.
    [DscProperty(Mandatory)]
    [Hashtable]$Settings

    [DscProperty()]
    [WinGetAction]$Action = [WinGetAction]::Partial

    # Gets the current UserSettings by looking at the settings.json file for the current user.
    [WinGetUserSettingsResource] Get()
    {
        $userSettings = Get-WinGetUserSettings
        $s = Get-UserSid
        $result = @{
            SID = $s
            Settings = $userSettings
        }
        return $result
    }

    # Tests if desired properties match. See notes on Overwrite.
    [bool] Test()
    {
        if ($this.Action -eq [WinGetAction]::Partial)
        {
            return Test-WinGetUserSettings -UserSettings $this.Settings
        }

        return Test-WinGetUserSettings -UserSettings $this.Settings -Full
    }

    # Sets the desired properties. See notes on Overwrite.
    [void] Set()
    {
        if ($this.Action -eq [WinGetAction]::Partial)
        {
            Set-WinGetUserSettings -UserSettings $this.Settings | Out-Null
        }
        else
        {
            Set-WinGetUserSettings -UserSettings $this.Settings -Overwrite | Out-Null
        }
    }
}

[DSCResource()]
class WinGetSourcesResource
{
    # We need a key. Do not set.
    [DscProperty(Key)]
    [string]$SID

    # An array of WinGetSource.
    [DscProperty(Mandatory)]
    [WinGetSource[]]$Sources

    [DscProperty()]
    [SourceCommand]$Command = [SourceCommand]::Add

    [DscProperty()]
    [WinGetAction]$Action = [WinGetAction]::Partial

    # Gets the current sources on winget.
    [WinGetSourcesResource] Get()
    {
        $packageCatalogReferences = Get-WinGetSource
        $wingetSources = [List[WinGetSource]]::new()
        foreach ($packageCatalogReference in $packageCatalogReferences)
        {
            $source = [WinGetSource]::new(
                $packageCatalogReference.Info.Name,
                $packageCatalogReference.Info.Argument,
                $packageCatalogReference.Info.Type)
            $wingetSources.Add($source)
        }

        $s = Get-UserSid
        $result = @{
            SID = $s
            Sources = $wingetSources
        }
        return $result
    }

    # Tests if desired properties match.
    [bool] Test()
    {
        $currentSources = $this.Get().Sources

        # If this is a full match and the counts are different give up.
        if (($this.Action -eq [WinGetAction]::Full) -and ($this.Sources.Count -ne $currentSources.Count))
        {
            return $false
        }

        # There's no need to differentiate between Partial and Full anymore.
        foreach ($source in $this.Sources)
        {
            if ([string]::IsNullOrWhiteSpace($source.Name))
            {
                throw "Invalid source"
            }

            # These fields must match.
            $result = $currentSources | Where-Object { $_.Name -eq $source.Name -and $_.Argument -eq $source.Argument -and $_.Type -eq $source.Type }

            # Source not found.
            if ($null -eq $result)
            {
                return $false
            }
        }

        return $true
    }

    # Sets the desired properties.
    [void] Set()
    {
        Assert-IsAdministrator

        foreach ($source in $this.Sources)
        {
            if ([string]::IsNullOrWhiteSpace($source.Name))
            {
                throw "Invalid source"
            }

            # Let winget.exe figure out if the source already exists or not.
            if ($this.Command -eq [SourceCommand]::Add)
            {
                Add-WinGetSource -Name $source.Name -Argument $source.Argument -Type $source.Type
            }
            else
            {
                Remove-WinGetSource -Name $source.Name
            }
        }
    }
}

#endregion DscResources
