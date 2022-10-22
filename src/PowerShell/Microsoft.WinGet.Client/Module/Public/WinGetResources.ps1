# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

enum UserSettingsMode
{
    Override
    Partial
}

[DSCResource()]
class UserSettings
{
    # Do not set.
    [DscProperty(Key)]
    [string]$SID

    [DscProperty(Mandatory)]
    [object]$Settings

    [DscProperty(Mandatory)]
    [UserSettingsMode]$Mode

    [UserSettings] Get()
    {
        $settingsPath = [Microsoft.WinGet.Client.DscResouces.UserSettings]::GetWinGetSettingsFilePath
        $settingsObj = Get-Content $settingsPath | Out-String | ConvertFrom-Json
        $s = Get-UserSid
        $result = @{
            SID = $s
            Settings = $settingsObj
        }
        return $result
    }

    [bool] Test()
    {
        # TODO: convert from UserSettingsMode to managed type
        $userSettingsObj = New-Object Microsoft.WinGet.Client.DscResouces.UserSettings($this.Settings)
        return $userSettingsObj.Test()
    }

    [void] Set()
    {
        # TODO: convert from UserSettingsMode to managed type
        $userSettingsObj = New-Object Microsoft.WinGet.Client.DscResouces.UserSettings($this.Settings)
        $userSettingsObj.Set()
    }
}

function Get-UserSid
{
    return (whoami /user /FO csv | ConvertFrom-Csv).SID
}
