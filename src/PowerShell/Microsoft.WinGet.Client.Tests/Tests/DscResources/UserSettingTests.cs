// -----------------------------------------------------------------------------
// <copyright file="UserSettingTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. Licensed under the MIT License.
// </copyright>
// -----------------------------------------------------------------------------

namespace Microsoft.WinGet.Client.Tests.Tests.DscResources
{
    using System;
    using System.IO;
    using Microsoft.WinGet.Client.DscResouces;
    using Newtonsoft.Json.Linq;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Tests for UserSettings. Uses fake temporary files as the winget settings file.
    /// </summary>
    public class UserSettingTests : IDisposable
    {
        private readonly ITestOutputHelper log;
        private readonly string mockFileDirectory;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserSettingTests"/> class.
        /// </summary>
        /// <param name="log">UT log.</param>
        public UserSettingTests(ITestOutputHelper log)
        {
            this.log = log;
            this.mockFileDirectory = Path.Combine(
                Path.GetTempPath(),
                Guid.NewGuid().ToString());
            Directory.CreateDirectory(this.mockFileDirectory);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Directory.Delete(this.mockFileDirectory, true);
        }

        [Fact]
        public void UserSettings_GoodInput()
        {
            var userSettingsFile = Path.Combine(this.mockFileDirectory, Path.GetRandomFileName());

            var inputSettings = new
            {
                source = new
                {
                    autoUpdateIntervalInMinutes = 5,
                },
                visual = new
                {
                    progressBar = "rainbow",
                },
                installBehavior = new
                {
                    preferences = new
                    {
                        scope = "user",
                        locale = new string[] { "en-US", "es-Mx" },
                    },
                },
                telemetry = new
                {
                    disable = true,
                },
            };

            _ = new UserSettings(inputSettings, UserSettings.ResourceMode.Full, userSettingsFile);

            this.log.WriteLine($"Result Content");
        }

        /*
        [Fact]
        public void UserSettings_GoodInput_ToCamelCase()
        {
        }

        [Fact]
        public void UserSettings_EmptyInput()
        {
        }

        [Fact]
        public void UserSettings_Null()
        {
        }

        [Fact]
        public void UserSettings_BadInput()
        {
        }

        [Fact]
        public void UserSettings_Set_WithSchema()
        {
        }

        [Fact]
        public void UserSettings_Set_WithoutSchema()
        {
        }

        [Fact]
        public void UserSettings_Set_Full_NewFile()
        {
        }

        [Fact]
        public void UserSettings_Set_Full_ExistingFile()
        {
        }

        [Fact]
        public void UserSettings_Set_Partial_EmptyFile()
        {
        }

        [Fact]
        public void UserSettings_Set_Partial_Merge()
        {
        }

        [Fact]
        public void UserSettings_Set_Partial_MergeAndOverwrite()
        {
        }

        [Fact]
        public void UserSettings_Test_Full_Equal()
        {
        }

        [Fact]
        public void UserSettings_Test_Full_NotEqual()
        {
        }

        [Fact]
        public void UserSettings_Test_Partial_Equal()
        {
        }

        [Fact]
        public void UserSettings_Test_Partial_NotEqual()
        {
        }

        [Fact]
        public void UserSettings_Get()
        {
            // not null?
        }
        */
    }
}
