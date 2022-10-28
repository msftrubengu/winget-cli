# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

function Get-UserSid
{
    return (whoami /user /FO csv | ConvertFrom-Csv).SID
}

function Get-WingetSources
{
    $sourcesJson = cmd /c winget.exe source export | ConvertFrom-Json
    return [WinGetSource[]]$sourcesJson
}

function Assert-IsAdministrator
{
    # Check that we are running as an administrator
    $windowsIdentity = [System.Security.Principal.WindowsIdentity]::GetCurrent()
    $windowsPrincipal = New-Object -TypeName 'System.Security.Principal.WindowsPrincipal' -ArgumentList @( $windowsIdentity )

    $adminRole = [System.Security.Principal.WindowsBuiltInRole]::Administrator

    if (-not $windowsPrincipal.IsInRole($adminRole))
    {
        New-InvalidOperationException -Message "This resource must run as an Administrator."
    }
}

function Assert-LastExitCode
{
    if ($LASTEXITCODE)
    {
        throw "Error running winget.exe exit code: '$LASTEXITCODE'"
    }
}
