// -----------------------------------------------------------------------------
// <copyright file="TestUserSettingsCommand.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. Licensed under the MIT License.
// </copyright>
// -----------------------------------------------------------------------------

namespace Microsoft.WinGet.Client.Commands
{
    using System;
    using System.Collections;
    using System.Management.Automation;
    using Microsoft.WinGet.Client.Common;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Sets the specified user settings into the winget user settings. If overwrite, then deletes previous settings
    /// and add the new ones. Otherwise merge them, if there's a conflict in the settings keep new configuration.
    /// </summary>
    [Cmdlet(VerbsDiagnostic.Test, Constants.Nouns.UserSettings)]
    [OutputType(typeof(bool))]
    public sealed class TestUserSettingsCommand : BaseUserSettingsCommand
    {
        /// <summary>
        /// Gets or sets the input user settings.
        /// </summary>
        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true)]
        public Hashtable UserSettings { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to test is full or not.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true)]
        public SwitchParameter Full { get; set; }

        /// <summary>
        /// Updates a package from the pipeline or from the local system.
        /// </summary>
        protected override void ProcessRecord()
        {
            this.WriteObject(this.CompareUserSettings());
        }

        private bool CompareUserSettings()
        {
            var currentSettings = LocalSettingsFileToJObject();
            var newSettings = HashtableToJObject(this.UserSettings);

            // Don't fail because of the schema.
            if (currentSettings.ContainsKey(SchemaKey))
            {
                currentSettings.Remove(SchemaKey);
            }

            if (newSettings.ContainsKey(SchemaKey))
            {
                newSettings.Remove(SchemaKey);
            }

            if (this.Full.ToBool())
            {
                return JToken.DeepEquals(newSettings, currentSettings);
            }

            return this.PartialCompare(newSettings, currentSettings);
        }

        private bool PartialCompare(JObject jObject, JObject other)
        {
            try
            {
                this.PartialDeepEquals(jObject, other);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        // This doesn't support JArray object comparison, but we don't have arrays of type object so far.
        private void PartialDeepEquals(JToken jToken, JToken other)
        {
            if (jToken.Type != other.Type)
            {
                string error = $"Mismatch types '{jToken.ToString(Newtonsoft.Json.Formatting.None)}' " +
                    $"'{other.ToString(Newtonsoft.Json.Formatting.None)}'";
                this.WriteVerbose(error);
                throw new Exception(error);
            }

            if (!JToken.DeepEquals(jToken, other))
            {
                if (jToken.Type == JTokenType.Object)
                {
                    var jObject = (JObject)jToken;
                    var otherJObject = (JObject)other;

                    var properties = jObject.Properties();
                    foreach (var property in properties)
                    {
                        // If the property is not there then give up.
                        if (!otherJObject.ContainsKey(property.Name))
                        {
                            string error = $"{property.Name} not found.";
                            this.WriteVerbose(error);
                            throw new Exception(error);
                        }

                        this.PartialDeepEquals(
                                property.Value,
                                otherJObject.GetValue(property.Name));
                    }
                }
                else if (jToken is JValue)
                {
                    // If this is a JValue (string, integer, date, etc) and DeepEquals fails then is not equal.
                    string error = $"'{jToken.ToString(Newtonsoft.Json.Formatting.None)}' != " +
                        $"'{other.ToString(Newtonsoft.Json.Formatting.None)}'";
                    this.WriteVerbose(error);
                    throw new Exception(error);
                }
            }
        }
    }
}
