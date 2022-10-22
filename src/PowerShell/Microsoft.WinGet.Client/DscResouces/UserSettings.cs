// -----------------------------------------------------------------------------
// <copyright file="UserSettings.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. Licensed under the MIT License.
// </copyright>
// -----------------------------------------------------------------------------

namespace Microsoft.WinGet.Client.DscResouces
{
    using System;
    using System.IO;
    using System.Linq;
    using Microsoft.WinGet.Client.Exceptions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// WinGet user settings DscResource.
    /// </summary>
    public class UserSettings
    {
        private const string WinGetSettingsFilePath = @"%LocalAppData%\Packages\Microsoft.DesktopAppInstaller_8wekyb3d8bbwe\LocalState\settings.json";
        private const string SchemaKey = "$schema";
        private const string SchemaValue = "https://aka.ms/winget-settings.schema.json";

        private readonly string userFileSettingsPath;
        private readonly JObject jsonSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserSettings"/> class.
        /// </summary>
        /// <param name="settings">Settings object.</param>
        /// <param name="resourceMode">Resource mode.</param>
        public UserSettings(object settings, ResourceMode resourceMode)
            : this(settings, resourceMode, UserSettings.GetWinGetSettingsFilePath())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserSettings"/> class.
        /// Used for testing.
        /// </summary>
        /// <param name="settings">Settings object.</param>
        /// <param name="resourceMode">Resource mode.</param>
        /// <param name="userFileSettingsPath">File settings path.</param>
        internal UserSettings(object settings, ResourceMode resourceMode, string userFileSettingsPath)
        {
            this.Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.jsonSettings = (JObject)JToken.FromObject(this.Settings);

            this.userFileSettingsPath = userFileSettingsPath;
            this.Mode = resourceMode;
        }

        /// <summary>
        /// Resource mode.
        /// </summary>
        public enum ResourceMode
        {
            /// <summary>
            /// Deletes previous settings and apply only the specified.
            /// </summary>
            Full,

            /// <summary>
            /// Appends configuration to previous user settings file. Overrites existing values.
            /// </summary>
            Partial,
        }

        /// <summary>
        /// Gets the settings of this resource.
        /// </summary>
        public object Settings { get; private set; }

        /// <summary>
        /// Gets the resource mode.
        /// </summary>
        public ResourceMode Mode { get; private set; }

        /// <summary>
        /// Gets the full path of the winget settings file.
        /// </summary>
        /// <returns>Settings file full path.</returns>
        public static string GetWinGetSettingsFilePath()
        {
            return Environment.ExpandEnvironmentVariables(WinGetSettingsFilePath);
        }

        /// <summary>
        /// Determines if the user settings node is currently compliant with the resource's desired state.
        /// </summary>
        /// <returns>True if compliant.</returns>
        public bool Test()
        {
            var fileSettings = this.ConvertSettingsFileToJObject();
            var jObject = (JObject)this.jsonSettings.DeepClone();

            // Don't fail because of the schema.
            if (fileSettings.ContainsKey(SchemaKey))
            {
                fileSettings.Remove(SchemaKey);
            }

            if (jObject.ContainsKey(SchemaKey))
            {
                jObject.Remove(SchemaKey);
            }

            if (this.Mode == ResourceMode.Partial)
            {
                return this.PartialCompare(jObject, fileSettings);
            }

            return JToken.DeepEquals(jObject, fileSettings);
        }

        /// <summary>
        /// Attempts to force the user settings node to become compliant with the resource's desired state.
        /// </summary>
        public void Set()
        {
            var jObject = (JObject)this.jsonSettings.DeepClone();

            // Merge settings.
            if (this.Mode == ResourceMode.Partial)
            {
                var fileSettings = this.ConvertSettingsFileToJObject();

                // To make the input setting to triumph, they have to be merged into the existing JObject.
                fileSettings.Merge(jObject, new JsonMergeSettings
                {
                    MergeArrayHandling = MergeArrayHandling.Union,
                    MergeNullValueHandling = MergeNullValueHandling.Ignore,
                });

                jObject = fileSettings;
            }

            if (!jObject.ContainsKey(SchemaKey))
            {
                jObject.Add(SchemaKey, SchemaValue);
            }

            var orderedJObject = this.CreateAlphabeticallyOrderedJObject(jObject);
            var json = orderedJObject.ToString(Formatting.Indented);

            File.WriteAllText(
                this.userFileSettingsPath,
                json);
        }

        private JObject ConvertSettingsFileToJObject()
        {
            if (!File.Exists(this.userFileSettingsPath))
            {
                return new JObject();
            }

            return JObject.Parse(File.ReadAllText(this.userFileSettingsPath));
        }

        /// <summary>
        /// Helper method to order alphabetically properties. Newtonsoft doesn't have a nice way
        /// to do it via a custom JsonConverter.
        /// </summary>
        /// <param name="jObject">JObject</param>
        /// <returns>New ordered JObject.</returns>
        private JObject CreateAlphabeticallyOrderedJObject(JObject jObject)
        {
            JObject newJObject = new ();
            var orderedProperties = jObject.Properties().OrderBy(p => p.Name, StringComparer.Ordinal);
            foreach (var property in orderedProperties)
            {
                if (property.Value.Type == JTokenType.Object)
                {
                    newJObject.Add(
                        property.Name,
                        this.CreateAlphabeticallyOrderedJObject((JObject)property.Value));
                }
                else
                {
                    newJObject.Add(property);
                }
            }

            return newJObject;
        }

        private bool PartialCompare(JObject jObject, JObject other)
        {
            try
            {
                this.PartialDeepEquals(jObject, other);
            }
            catch (DscResourceException)
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
                throw new DscResourceException();
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
                            throw new DscResourceException();
                        }

                        this.PartialDeepEquals(
                                property.Value,
                                otherJObject.GetValue(property.Name));
                    }
                }
                else if (jToken is JValue)
                {
                    // If this is a JValue (string, integer, date, etc) and DeepEquals fails then is not equal.
                    throw new DscResourceException();
                }
            }
        }
    }
}
