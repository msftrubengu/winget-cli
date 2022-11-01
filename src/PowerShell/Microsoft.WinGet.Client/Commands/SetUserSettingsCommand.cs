// -----------------------------------------------------------------------------
// <copyright file="SetUserSettingsCommand.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. Licensed under the MIT License.
// </copyright>
// -----------------------------------------------------------------------------

namespace Microsoft.WinGet.Client.Commands
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using Microsoft.WinGet.Client.Common;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Sets the specified user settings into the winget user settings. If overwrite, then deletes previous settings
    /// and add the new ones. Otherwise merge them, if there's a conflict in the settings keep new configuration.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, Constants.Nouns.UserSettings)]
    [OutputType(typeof(Hashtable))]
    public sealed class SetUserSettingsCommand : BaseUserSettingsCommand
    {
        /// <summary>
        /// Gets or sets the input user settings.
        /// </summary>
        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true)]
        public Hashtable UserSettings { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to continue upon non security related failures.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true)]
        public SwitchParameter Overwrite { get; set; }

        /// <summary>
        /// Updates a package from the pipeline or from the local system.
        /// </summary>
        protected override void ProcessRecord()
        {
            var newSettings = HashtableToJObject(this.UserSettings);

            // Merge settings.
            if (!this.Overwrite.ToBool())
            {
                var currentSettings = LocalSettingsFileToJObject();

                // To make the input setting to triumph, input user settings need to be merged into the existing settings.
                currentSettings.Merge(newSettings, new JsonMergeSettings
                {
                    MergeArrayHandling = MergeArrayHandling.Union,
                    MergeNullValueHandling = MergeNullValueHandling.Ignore,
                });

                newSettings = currentSettings;
            }

            // Add schema if not there.
            if (!newSettings.ContainsKey(SchemaKey))
            {
                newSettings.Add(SchemaKey, SchemaValue);
            }

            var orderedSettings = CreateAlphabeticallyOrderedJObject(newSettings);

            // Write settings.
            File.WriteAllText(
                WinGetSettingsFilePath,
                orderedSettings.ToString(Formatting.Indented));

            this.WriteObject(LocalSettingsFileToHashtable());
        }

        /// <summary>
        /// Helper method to order alphabetically properties. Newtonsoft doesn't have a nice way
        /// to do it via a custom JsonConverter.
        /// </summary>
        /// <param name="jObject">JObject</param>
        /// <returns>New ordered JObject.</returns>
        private static JObject CreateAlphabeticallyOrderedJObject(JObject jObject)
        {
            JObject newJObject = new ();
            var orderedProperties = jObject.Properties().OrderBy(p => p.Name, StringComparer.Ordinal);
            foreach (var property in orderedProperties)
            {
                if (property.Value.Type == JTokenType.Object)
                {
                    newJObject.Add(
                        property.Name,
                        CreateAlphabeticallyOrderedJObject((JObject)property.Value));
                }
                else
                {
                    newJObject.Add(property);
                }
            }

            return newJObject;
        }
    }
}
