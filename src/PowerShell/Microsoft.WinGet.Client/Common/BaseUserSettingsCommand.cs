// -----------------------------------------------------------------------------
// <copyright file="BaseUserSettingsCommand.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. Licensed under the MIT License.
// </copyright>
// -----------------------------------------------------------------------------

namespace Microsoft.WinGet.Client.Common
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Base command for user settings cmdlets.
    /// </summary>
    public abstract class BaseUserSettingsCommand : PSCmdlet
    {
        /// <summary>
        /// The schema key.
        /// </summary>
        protected const string SchemaKey = "$schema";

        /// <summary>
        /// The default value of the schema property.
        /// </summary>
        protected const string SchemaValue = "https://aka.ms/winget-settings.schema.json";

        private const string LocalAppDataWinGetSettingsPath = @"%LocalAppData%\Packages\Microsoft.DesktopAppInstaller_8wekyb3d8bbwe\LocalState\settings.json";

        /// <summary>
        /// Gets the path for the winget settings path expanding LocalAppData.
        /// </summary>
        protected static string WinGetSettingsFilePath
        {
            get
            {
                return Environment.ExpandEnvironmentVariables(LocalAppDataWinGetSettingsPath);
            }
        }

        /// <summary>
        /// Converts a Hashtable to a JObject object.
        /// </summary>
        /// <param name="hashtable">Hashtable.</param>
        /// <returns>JObject.</returns>
        protected static JObject HashtableToJObject(Hashtable hashtable)
        {
            return (JObject)JToken.FromObject(hashtable);
        }

        /// <summary>
        /// Converts the current local settings file into a Hashtable object.
        /// </summary>
        /// <returns>User settings as hash table.</returns>
        protected static Hashtable LocalSettingsFileToHashtable()
        {
            if (!File.Exists(WinGetSettingsFilePath))
            {
                return new Hashtable();
            }

            return JsonConvert.DeserializeObject<Hashtable>(
                File.ReadAllText(WinGetSettingsFilePath));
        }

        /// <summary>
        /// Converts the current local settings file into a JObject object.
        /// </summary>
        /// <returns>User settings as JObject.</returns>
        protected static JObject LocalSettingsFileToJObject()
        {
            if (!File.Exists(WinGetSettingsFilePath))
            {
                return new JObject();
            }

            return JObject.Parse(File.ReadAllText(WinGetSettingsFilePath));
        }
    }
}
