// -----------------------------------------------------------------------------
// <copyright file="GetUserSettingsCommand.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. Licensed under the MIT License.
// </copyright>
// -----------------------------------------------------------------------------

namespace Microsoft.WinGet.Client.Commands
{
    using System.Collections;
    using System.Management.Automation;
    using Microsoft.WinGet.Client.Common;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Sets the specified user settings into the winget user settings. If overwrite, then deletes previous settings
    /// and add the new ones. Otherwise merge them, if there's a conflict in the settings keep new configuration.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, Constants.Nouns.UserSettings)]
    [OutputType(typeof(Hashtable))]
    public sealed class GetUserSettingsCommand : BaseUserSettingsCommand
    {
        /// <summary>
        /// Updates a package from the pipeline or from the local system.
        /// </summary>
        protected override void ProcessRecord()
        {
            this.WriteObject(LocalSettingsFileToHashtable());
        }
    }
}
