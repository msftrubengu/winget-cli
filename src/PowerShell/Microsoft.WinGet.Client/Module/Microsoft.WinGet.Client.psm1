# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

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

#region PowerShell Classes
# Classes that are available to users outside of this module should be defined in the root module.
# To load them use `using module Microsoft.WinGet.Client`

# The source data as exported from by running `winget source export`
# WinGet treats an empty type as Microsoft.PreIndexed.Package.
class WinGetSource
{
    [string]$Name;
    [string]$Arg;
    [string]$Type = "Microsoft.PreIndexed.Package";
    [string]$Identifier;
    [string]$Data;

    # We need to explicity the default constructor
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
        $this.Arg = $a;
    }

    WinGetSource([string]$n, [string]$a, [string]$t)
    {
        $this.Name = $n;
        $this.Arg = $a;
        $this.Type = $t;
    }
}

#endregion PowerShell Classes

#region DscResources
# DSC Powershell doesn't support binary DSC resources without the MOF schema. Author here all DSC Resources.

# This resource is in charge of editing the settings.json file of winget.
[DSCResource()]
class UserSettingsResource
{
    # We need a key. Do not set.
    [DscProperty(Key)]
    [string]$SID

    # A hash table with the desired settings.
    [DscProperty(Mandatory)]
    [Hashtable]$Settings

    # For Set - if true, overwrites the entire settings.json file. Otherwise, appends the desired settings and/or modified
    #           the settings if already exists.
    # For Test - if true, test all properties are set with the specified value. Otherwise, verifies the desired properties
    #            are set with the defined value.
    [DscProperty()]
    [bool]$Overwrite = $false

    # Gets the current UserSettings by looking at the settings.json file for the current user.
    [UserSettingsResource] Get()
    {
        $settingsPath = [Microsoft.WinGet.Client.DscResouces.UserSettings]::GetWinGetSettingsFilePath()
        $userSettingsObj = New-Object Microsoft.WinGet.Client.DscResouces.UserSettings($settingsPath, $this.Overwrite)
        $userSettingsGet = $userSettingsObj.Get()
        $s = Get-UserSid
        $result = @{
            SID = $s
            Settings = $userSettingsGet.Settings
            Overwrite = $userSettingsGet.Overwrite
        }
        return $result
    }

    # Tests if desired properties match. See notes on Overwrite.
    [bool] Test()
    {
        $settingsPath = [Microsoft.WinGet.Client.DscResouces.UserSettings]::GetWinGetSettingsFilePath()
        $userSettingsObj = New-Object Microsoft.WinGet.Client.DscResouces.UserSettings($this.Settings, $settingsPath, $this.Overwrite)
        return $userSettingsObj.Test()
    }

    # Sets the desired properties. See notes on Overwrite.
    [void] Set()
    {
        $settingsPath = [Microsoft.WinGet.Client.DscResouces.UserSettings]::GetWinGetSettingsFilePath()
        $userSettingsObj = New-Object Microsoft.WinGet.Client.DscResouces.UserSettings($this.Settings, $settingsPath, $this.Overwrite)
        $userSettingsObj.Set()
    }
}

enum SourceCommand
{
    Add
    Remove
}

[DSCResource()]
class SourcesResource
{
    # We need a key. Do not set.
    [DscProperty(Key)]
    [string]$SID

    [DscProperty(Mandatory)]
    [WinGetSource[]]$Sources

    [SourceCommand]$Command = [SourceCommand]::Add

    [SourcesResource] Get()
    {
        $wingetSources = Get-WingetSources
        Assert-LastExitCode

        $s = Get-UserSid
        $result = @{
            SID = $s
            Sources = $wingetSources
            Command = $this.Command
        }
        return $result
    }

    [bool] Test()
    {
        $otherSources = $this.Get()

        foreach ($source in $this.Sources)
        {
            if ([string]::IsNullOrWhiteSpace($source.Name))
            {
                throw "Invalid source"
            }

            # These fields must match.
            $result = $otherSources | Where-Object
            {
                $_.Name -eq $source.Name -and
                $_.Arg -eq $source.Arg -and
                $_.Type -eq $source.Type
            }

            # Source not found.
            if ($null -eq $result)
            {
                return $false
            }
        }

        return $true
    }

    [void] Set()
    {
        Assert-IsAdministrator

        foreach ($source in $this.Sources)
        {
            if ($this.Command -eq [SourceCommand]::Add)
            {
                winget.exe source add --name $source.Name --arg $source.Arg --type $source.Type
            }
            else
            {
                winget.exe source remove --name $source.Name
            }
        }
    }
}

#endregion DscResources
