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

        /// <summary>
        /// Tests with and empty object.
        /// </summary>
        [Fact]
        public void UserSettings_EmptyInput()
        {
            var userSettingsFile = Path.Combine(this.mockFileDirectory, Path.GetRandomFileName());

            var inputSettings = new
            {
            };

            _ = new UserSettings(inputSettings, UserSettings.ResourceMode.Full, userSettingsFile);
        }

        /// <summary>
        /// Tests with null.
        /// </summary>
        [Fact]
        public void UserSettings_Null()
        {
            var userSettingsFile = Path.Combine(this.mockFileDirectory, Path.GetRandomFileName());
            Assert.Throws<ArgumentNullException>(
                () => _ = new UserSettings(null, UserSettings.ResourceMode.Full, userSettingsFile));
        }

        /// <summary>
        /// Tests bad input.
        /// </summary>
        /// <param name="input">Bad input</param>
        [Theory]
        [InlineData(5)]
        [InlineData("data")]
        public void UserSettings_BadInput(object input)
        {
            var userSettingsFile = Path.Combine(this.mockFileDirectory, Path.GetRandomFileName());

            Assert.Throws<InvalidCastException>(
                () => _ = new UserSettings(input, UserSettings.ResourceMode.Full, userSettingsFile));
        }

        /// <summary>
        /// Tests Set with an empty existing setting file.
        /// </summary>
        [Fact]
        public void UserSettings_Set()
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
                        locale = new string[] { "en-US", "es-MX" },
                    },
                },
                telemetry = new
                {
                    disable = true,
                },
            };

            string expectedSettingsContentFile = @"{
  ""source"": {
    ""autoUpdateIntervalInMinutes"": 5
  },
  ""visual"": {
    ""progressBar"": ""rainbow""
  },
  ""installBehavior"": {
    ""preferences"": {
      ""scope"": ""user"",
      ""locale"": [
        ""en-US"",
        ""es-MX""
      ]
    }
  },
  ""telemetry"": {
    ""disable"": true
  },
  ""$schema"": ""https://aka.ms/winget-settings.schema.json""
}";

            var userSettings = new UserSettings(inputSettings, UserSettings.ResourceMode.Full, userSettingsFile);

            userSettings.Set();

            Assert.Equal(expectedSettingsContentFile, File.ReadAllText(userSettingsFile));
        }

        /// <summary>
        /// Tests Set when a settings file already exists. Must overrite it.
        /// </summary>
        [Fact]
        public void UserSettings_Set_Full_ExistingFile()
        {
            var userSettingsFile = Path.Combine(this.mockFileDirectory, Path.GetRandomFileName());

            string existingContentFile = @"{
  ""$schema"": ""https://aka.ms/winget-settings.schema.json"",
  ""logging"": {
    ""level"": [
      ""verbose"",
      ""info"",
      ""warning"",
      ""error"",
      ""critical""
    ]
  },
}";

            File.WriteAllText(userSettingsFile, existingContentFile);

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
                        locale = new string[] { "en-US", "es-MX" },
                    },
                },
                telemetry = new
                {
                    disable = true,
                },
            };

            string expectedSettingsContentFile = @"{
  ""source"": {
    ""autoUpdateIntervalInMinutes"": 5
  },
  ""visual"": {
    ""progressBar"": ""rainbow""
  },
  ""installBehavior"": {
    ""preferences"": {
      ""scope"": ""user"",
      ""locale"": [
        ""en-US"",
        ""es-MX""
      ]
    }
  },
  ""telemetry"": {
    ""disable"": true
  },
  ""$schema"": ""https://aka.ms/winget-settings.schema.json""
}";

            var userSettings = new UserSettings(inputSettings, UserSettings.ResourceMode.Full, userSettingsFile);

            userSettings.Set();

            Assert.Equal(expectedSettingsContentFile, File.ReadAllText(userSettingsFile));
        }

        /// <summary>
        /// Tests Set with empty input settings and an existing settings file.
        /// </summary>
        [Fact]
        public void UserSettings_Set_Partial_EmptySettings()
        {
            var userSettingsFile = Path.Combine(this.mockFileDirectory, Path.GetRandomFileName());

            string existingContentFile = @"{
  ""$schema"": ""https://aka.ms/winget-settings.schema.json"",
  ""logging"": {
    ""level"": [
      ""verbose"",
      ""info"",
      ""warning"",
      ""error"",
      ""critical""
    ]
  }
}";

            File.WriteAllText(userSettingsFile, existingContentFile);

            var inputSettings = new
            {
            };

            var userSettings = new UserSettings(inputSettings, UserSettings.ResourceMode.Partial, userSettingsFile);

            userSettings.Set();

            Assert.Equal(existingContentFile, File.ReadAllText(userSettingsFile));
        }

        /// <summary>
        /// Tests Set with partial merge and no collisions.
        /// </summary>
        [Fact]
        public void UserSettings_Set_Partial_Merge()
        {
            var userSettingsFile = Path.Combine(this.mockFileDirectory, Path.GetRandomFileName());

            string existingContentFile = @"{
  ""$schema"": ""https://aka.ms/winget-settings.schema.json"",
  ""logging"": {
    ""level"": [
      ""verbose"",
      ""info"",
      ""warning"",
      ""error"",
      ""critical""
    ]
  },
}";

            File.WriteAllText(userSettingsFile, existingContentFile);

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
                        locale = new string[] { "en-US", "es-MX" },
                    },
                },
                telemetry = new
                {
                    disable = true,
                },
            };

            string expectedSettingsContentFile = @"{
  ""$schema"": ""https://aka.ms/winget-settings.schema.json"",
  ""logging"": {
    ""level"": [
      ""verbose"",
      ""info"",
      ""warning"",
      ""error"",
      ""critical""
    ]
  },
  ""source"": {
    ""autoUpdateIntervalInMinutes"": 5
  },
  ""visual"": {
    ""progressBar"": ""rainbow""
  },
  ""installBehavior"": {
    ""preferences"": {
      ""scope"": ""user"",
      ""locale"": [
        ""en-US"",
        ""es-MX""
      ]
    }
  },
  ""telemetry"": {
    ""disable"": true
  }
}";

            var userSettings = new UserSettings(inputSettings, UserSettings.ResourceMode.Partial, userSettingsFile);

            userSettings.Set();

            Assert.Equal(expectedSettingsContentFile, File.ReadAllText(userSettingsFile));
        }

        /// <summary>
        /// Tests Set with partial merge with collisions. Input must triumph.
        /// </summary>
        [Fact]
        public void UserSettings_Set_Partial_MergeAndOverwrite()
        {
            var userSettingsFile = Path.Combine(this.mockFileDirectory, Path.GetRandomFileName());

            string existingContentFile = @"{
  ""$schema"": ""https://aka.ms/winget-settings.schema.json"",
  ""logging"": {
    ""level"": [
      ""verbose"",
      ""info"",
      ""warning"",
      ""error"",
      ""critical""
    ]
  },
  ""installBehavior"": {
    ""preferences"": {
      ""locale"": [
        ""fr-FR"",
      ]
    }
  },
  ""telemetry"": {
    ""disable"": false
  },
  ""source"": {
    ""autoUpdateIntervalInMinutes"": 15
  },
}";

            File.WriteAllText(userSettingsFile, existingContentFile);

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
                        locale = new string[] { "en-US", "es-MX" },
                    },
                },
                telemetry = new
                {
                    disable = true,
                },
            };

            string expectedSettingsContentFile = @"{
  ""$schema"": ""https://aka.ms/winget-settings.schema.json"",
  ""logging"": {
    ""level"": [
      ""verbose"",
      ""info"",
      ""warning"",
      ""error"",
      ""critical""
    ]
  },
  ""installBehavior"": {
    ""preferences"": {
      ""locale"": [
        ""fr-FR"",
        ""en-US"",
        ""es-MX""
      ],
      ""scope"": ""user""
    }
  },
  ""telemetry"": {
    ""disable"": true
  },
  ""source"": {
    ""autoUpdateIntervalInMinutes"": 5
  },
  ""visual"": {
    ""progressBar"": ""rainbow""
  }
}";

            var userSettings = new UserSettings(inputSettings, UserSettings.ResourceMode.Partial, userSettingsFile);

            userSettings.Set();

            Assert.Equal(expectedSettingsContentFile, File.ReadAllText(userSettingsFile));
        }

        /*
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
