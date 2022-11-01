# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

function Get-UserSid
{
    return (whoami /user /FO csv | ConvertFrom-Csv).SID
}

# Check that we are running as an administrator
function Assert-IsAdministrator
{
    $windowsIdentity = [System.Security.Principal.WindowsIdentity]::GetCurrent()
    $windowsPrincipal = New-Object -TypeName 'System.Security.Principal.WindowsPrincipal' -ArgumentList @( $windowsIdentity )

    $adminRole = [System.Security.Principal.WindowsBuiltInRole]::Administrator

    if (-not $windowsPrincipal.IsInRole($adminRole))
    {
        New-InvalidOperationException -Message "This resource must run as an Administrator."
    }
}
