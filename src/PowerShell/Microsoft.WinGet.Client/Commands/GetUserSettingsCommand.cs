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
    /// Gets winget's user settings as a Hashtable.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, Constants.Nouns.UserSettings)]
    [OutputType(typeof(Hashtable))]
    public sealed class GetUserSettingsCommand : BaseUserSettingsCommand
    {
        /// <summary>
        /// Writes the settings file as a Hashtable.
        /// </summary>
        protected override void ProcessRecord()
        {
            this.WriteObject(LocalSettingsFileToHashtable());
        }
    }
}
