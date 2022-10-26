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

[DSCResource()]
class UserSettings
{
    # Do not set.
    [DscProperty(Key)]
    [string]$SID

    [DscProperty(Mandatory)]
    [Hashtable]$Settings

    [DscProperty()]
    [bool]$Overwrite = $false

    [UserSettings] Get()
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

    [bool] Test()
    {
        $settingsPath = [Microsoft.WinGet.Client.DscResouces.UserSettings]::GetWinGetSettingsFilePath()
        $userSettingsObj = New-Object Microsoft.WinGet.Client.DscResouces.UserSettings($this.Settings, $settingsPath, $this.Overwrite)
        return $userSettingsObj.Test()
    }

    [void] Set()
    {
        $settingsPath = [Microsoft.WinGet.Client.DscResouces.UserSettings]::GetWinGetSettingsFilePath()
        $userSettingsObj = New-Object Microsoft.WinGet.Client.DscResouces.UserSettings($this.Settings, $settingsPath, $this.Overwrite)
        $userSettingsObj.Set()
    }
}

function Get-UserSid
{
    return (whoami /user /FO csv | ConvertFrom-Csv).SID
}
