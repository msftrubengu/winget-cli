// -----------------------------------------------------------------------------
// <copyright file="UserSettings.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. Licensed under the MIT License.
// </copyright>
// -----------------------------------------------------------------------------

namespace Microsoft.WinGet.Client.DscResouces
{
    using System;
    using System.IO;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// WinGet user settings DscResource.
    /// </summary>
    public class UserSettings
    {
        private const string UserSettingsLocation = @"%LocalAppData%\Packages\Microsoft.DesktopAppInstaller_8wekyb3d8bbwe\LocalState\settings.json";
        private const string SchemaKey = "$schema";
        private const string SchemaValue = "https://aka.ms/winget-settings.schema.json";

        private readonly string userFileSettingsPath;
        private readonly JObject jsonSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserSettings"/> class.
        /// </summary>
        /// <param name="settings">Settings object.</param>
        /// <param name="resourceMode">Resource mode.</param>
        public UserSettings(object settings, ResourceMode resourceMode = ResourceMode.Full)
            : this(settings, resourceMode, Environment.ExpandEnvironmentVariables(UserSettingsLocation))
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
        /// Retrieves the state of the resource the user settings resource.
        /// </summary>
        /// <returns>Source.</returns>
        public UserSettings Get()
        {
            // this or a new copy?
            return this;
        }

        /// <summary>
        /// Determines if the user settings node is currently compliant with the resource's desired state.
        /// </summary>
        /// <returns>True if compliant.</returns>
        public bool Test()
        {
            var fileSettings = this.ConvertSettingsFileToJObject();

            if (this.Mode == ResourceMode.Full)
            {
                return JToken.DeepEquals(this.jsonSettings, fileSettings);
            }

            // verify one by one. have to check for types. sounds hard.
            return false;
        }

        /// <summary>
        /// Attempts to force the user settings node to become compliant with the resource's desired state.
        /// </summary>
        public void Set()
        {
            // Merge settings.
            if (this.Mode == ResourceMode.Partial)
            {
                var fileSettings = this.ConvertSettingsFileToJObject();
                this.jsonSettings.Merge(fileSettings, new JsonMergeSettings
                {
                    MergeArrayHandling = MergeArrayHandling.Union,
                    MergeNullValueHandling = MergeNullValueHandling.Ignore,
                });
            }

            if (!this.jsonSettings.ContainsKey(SchemaKey))
            {
                this.jsonSettings.Add(SchemaKey, SchemaValue);
            }

            string serialized = JsonConvert.SerializeObject(
                this.jsonSettings,
                Formatting.Indented,
                new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

            File.WriteAllText(
                this.userFileSettingsPath,
                serialized);
        }

        private JObject ConvertSettingsFileToJObject()
        {
            if (!File.Exists(this.userFileSettingsPath))
            {
                return new JObject();
            }

            return JObject.Parse(this.userFileSettingsPath);
        }
    }
}
