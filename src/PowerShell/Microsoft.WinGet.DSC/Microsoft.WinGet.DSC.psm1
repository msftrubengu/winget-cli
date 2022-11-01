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

#region enums
enum WinGetAction
{
    Partial
    Full
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

#endregion DscResources
