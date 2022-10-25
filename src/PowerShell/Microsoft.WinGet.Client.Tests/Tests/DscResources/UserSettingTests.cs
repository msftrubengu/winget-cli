// -----------------------------------------------------------------------------
// <copyright file="UserSettingTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. Licensed under the MIT License.
// </copyright>
// -----------------------------------------------------------------------------

namespace Microsoft.WinGet.Client.Tests.Tests.DscResources
{
    using System;
    using System.Collections;
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

        /// <summary>
        /// Tests with and empty object.
        /// </summary>
        [Fact]
        public void UserSettings_EmptyInput()
        {
            var userSettingsFile = Path.Combine(this.mockFileDirectory, Path.GetRandomFileName());

            var inputSettings = new Hashtable();

            _ = new UserSettings(inputSettings, userSettingsFile, true);
        }

        /// <summary>
        /// Tests with null.
        /// </summary>
        [Fact]
        public void UserSettings_Null()
        {
            var userSettingsFile = Path.Combine(this.mockFileDirectory, Path.GetRandomFileName());
            Assert.Throws<ArgumentNullException>(
                () => _ = new UserSettings(null, userSettingsFile, true));
        }

        /// <summary>
        /// Tests Set with an empty existing setting file.
        /// </summary>
        [Fact]
        public void UserSettings_Set()
        {
            var userSettingsFile = Path.Combine(this.mockFileDirectory, Path.GetRandomFileName());

            var inputSettings = new Hashtable()
            {
                {
                    "source",
                    new Hashtable()
                    {
                        {
                            "autoUpdateIntervalInMinutes",
                            5
                        },
                    }
                },
                {
                    "visual",
                    new Hashtable()
                    {
                        {
                            "progressBar",
                            "rainbow"
                        },
                    }
                },
                {
                    "installBehavior",
                    new Hashtable()
                    {
                        {
                            "preferences",
                            new Hashtable()
                            {
                                {
                                    "scope",
                                    "user"
                                },
                                {
                                    "locale",
                                    new string[] { "en-US", "es-MX" }
                                },
                            }
                        },
                    }
                },
                {
                    "telemetry",
                    new Hashtable()
                    {
                        {
                            "disable",
                            true
                        },
                    }
                },
            };

            string expectedSettingsContentFile = @"{
  ""$schema"": ""https://aka.ms/winget-settings.schema.json"",
  ""installBehavior"": {
    ""preferences"": {
      ""locale"": [
        ""en-US"",
        ""es-MX""
      ],
      ""scope"": ""user""
    }
  },
  ""source"": {
    ""autoUpdateIntervalInMinutes"": 5
  },
  ""telemetry"": {
    ""disable"": true
  },
  ""visual"": {
    ""progressBar"": ""rainbow""
  }
}";

            var userSettings = new UserSettings(inputSettings, userSettingsFile, true);

            userSettings.Set();

            var result = File.ReadAllText(userSettingsFile);
            this.log.WriteLine(result);
            Assert.Equal(expectedSettingsContentFile, result);
        }

        /// <summary>
        /// Tests Set when a settings file already exists. Must overrite it.
        /// </summary>
        [Fact]
        public void UserSettings_Set_Overwrite_ExistingFile()
        {
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
            var userSettingsFile = this.SetUpSettingsFile(existingContentFile);

            var inputSettings = new Hashtable()
            {
                {
                    "source",
                    new Hashtable()
                    {
                        {
                            "autoUpdateIntervalInMinutes",
                            5
                        },
                    }
                },
                {
                    "visual",
                    new Hashtable()
                    {
                        {
                            "progressBar",
                            "rainbow"
                        },
                    }
                },
                {
                    "installBehavior",
                    new Hashtable()
                    {
                        {
                            "preferences",
                            new Hashtable()
                            {
                                {
                                    "scope",
                                    "user"
                                },
                                {
                                    "locale",
                                    new string[] { "en-US", "es-MX" }
                                },
                            }
                        },
                    }
                },
                {
                    "telemetry",
                    new Hashtable()
                    {
                        {
                            "disable",
                            true
                        },
                    }
                },
            };

            string expectedSettingsContentFile = @"{
  ""$schema"": ""https://aka.ms/winget-settings.schema.json"",
  ""installBehavior"": {
    ""preferences"": {
      ""locale"": [
        ""en-US"",
        ""es-MX""
      ],
      ""scope"": ""user""
    }
  },
  ""source"": {
    ""autoUpdateIntervalInMinutes"": 5
  },
  ""telemetry"": {
    ""disable"": true
  },
  ""visual"": {
    ""progressBar"": ""rainbow""
  }
}";

            var userSettings = new UserSettings(inputSettings, userSettingsFile, true);

            userSettings.Set();

            var result = File.ReadAllText(userSettingsFile);
            this.log.WriteLine(result);
            Assert.Equal(expectedSettingsContentFile, result);
        }

        /// <summary>
        /// Tests Set with empty input settings and an existing settings file.
        /// </summary>
        [Fact]
        public void UserSettings_Set_Partial_EmptySettings()
        {
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
            var userSettingsFile = this.SetUpSettingsFile(existingContentFile);

            var inputSettings = new Hashtable();

            var userSettings = new UserSettings(inputSettings, userSettingsFile, false);

            userSettings.Set();

            var result = File.ReadAllText(userSettingsFile);
            this.log.WriteLine(result);
            Assert.Equal(existingContentFile, result);
        }

        /// <summary>
        /// Tests Set with partial merge and no collisions.
        /// </summary>
        [Fact]
        public void UserSettings_Set_Partial_Partial()
        {
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
            var userSettingsFile = this.SetUpSettingsFile(existingContentFile);

            var inputSettings = new Hashtable()
            {
                {
                    "source",
                    new Hashtable()
                    {
                        {
                            "autoUpdateIntervalInMinutes",
                            5
                        },
                    }
                },
                {
                    "visual",
                    new Hashtable()
                    {
                        {
                            "progressBar",
                            "rainbow"
                        },
                    }
                },
                {
                    "installBehavior",
                    new Hashtable()
                    {
                        {
                            "preferences",
                            new Hashtable()
                            {
                                {
                                    "scope",
                                    "user"
                                },
                                {
                                    "locale",
                                    new string[] { "en-US", "es-MX" }
                                },
                            }
                        },
                    }
                },
                {
                    "telemetry",
                    new Hashtable()
                    {
                        {
                            "disable",
                            true
                        },
                    }
                },
            };

            string expectedSettingsContentFile = @"{
  ""$schema"": ""https://aka.ms/winget-settings.schema.json"",
  ""installBehavior"": {
    ""preferences"": {
      ""locale"": [
        ""en-US"",
        ""es-MX""
      ],
      ""scope"": ""user""
    }
  },
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
  ""telemetry"": {
    ""disable"": true
  },
  ""visual"": {
    ""progressBar"": ""rainbow""
  }
}";

            var userSettings = new UserSettings(inputSettings, userSettingsFile, false);

            userSettings.Set();

            var result = File.ReadAllText(userSettingsFile);
            this.log.WriteLine(result);
            Assert.Equal(expectedSettingsContentFile, result);
        }

        /// <summary>
        /// Tests Set with partial merge with collisions. Input must triumph.
        /// </summary>
        [Fact]
        public void UserSettings_Set_Partial_PartialAndOverwrite()
        {
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
            var userSettingsFile = this.SetUpSettingsFile(existingContentFile);

            var inputSettings = new Hashtable()
            {
                {
                    "source",
                    new Hashtable()
                    {
                        {
                            "autoUpdateIntervalInMinutes",
                            5
                        },
                    }
                },
                {
                    "visual",
                    new Hashtable()
                    {
                        {
                            "progressBar",
                            "rainbow"
                        },
                    }
                },
                {
                    "installBehavior",
                    new Hashtable()
                    {
                        {
                            "preferences",
                            new Hashtable()
                            {
                                {
                                    "scope",
                                    "user"
                                },
                                {
                                    "locale",
                                    new string[] { "en-US", "es-MX" }
                                },
                            }
                        },
                    }
                },
                {
                    "telemetry",
                    new Hashtable()
                    {
                        {
                            "disable",
                            true
                        },
                    }
                },
            };

            string expectedSettingsContentFile = @"{
  ""$schema"": ""https://aka.ms/winget-settings.schema.json"",
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
  ""telemetry"": {
    ""disable"": true
  },
  ""visual"": {
    ""progressBar"": ""rainbow""
  }
}";

            var userSettings = new UserSettings(inputSettings, userSettingsFile, false);

            userSettings.Set();

            var result = File.ReadAllText(userSettingsFile);
            this.log.WriteLine(result);
            Assert.Equal(expectedSettingsContentFile, result);
        }

        /// <summary>
        /// Tests UserSettings Test method Overwrite. Settings are equal.
        /// </summary>
        [Fact]
        public void UserSettings_Test_Overwrite_Equal()
        {
            string existingContentFile = @"{
  ""installBehavior"": {
    ""preferences"": {
      ""scope"": ""user"",
      ""locale"": [
        ""en-US"",
        ""es-MX"",
      ]
    }
  },
  ""telemetry"": {
    ""disable"": false
  },
  ""visual"": {
    ""progressBar"": ""rainbow""
  },
  ""source"": {
    ""autoUpdateIntervalInMinutes"": 5
  }
}";
            var userSettingsFile = this.SetUpSettingsFile(existingContentFile);

            var inputSettings = new Hashtable()
            {
                {
                    "source",
                    new Hashtable()
                    {
                        {
                            "autoUpdateIntervalInMinutes",
                            5
                        },
                    }
                },
                {
                    "visual",
                    new Hashtable()
                    {
                        {
                            "progressBar",
                            "rainbow"
                        },
                    }
                },
                {
                    "installBehavior",
                    new Hashtable()
                    {
                        {
                            "preferences",
                            new Hashtable()
                            {
                                {
                                    "scope",
                                    "user"
                                },
                                {
                                    "locale",
                                    new string[] { "en-US", "es-MX" }
                                },
                            }
                        },
                    }
                },
                {
                    "telemetry",
                    new Hashtable()
                    {
                        {
                            "disable",
                            false
                        },
                    }
                },
            };

            var userSettings = new UserSettings(inputSettings, userSettingsFile, true);

            Assert.True(userSettings.Test());
        }

        /// <summary>
        /// Tests UserSettings Test method Overwrite. Settings are not equal. Input contains more elements.
        /// </summary>
        [Fact]
        public void UserSettings_Test_Overwrite_NotEqual_MoreInputProperties()
        {
            string existingContentFile = @"{
  ""$schema"": ""https://aka.ms/winget-settings.schema.json"",
  ""installBehavior"": {
    ""preferences"": {
      ""scope"": ""user"",
      ""locale"": [
        ""en-US"",
        ""es-MX"",
      ]
    }
  },
  ""visual"": {
    ""progressBar"": ""rainbow""
  },
  ""source"": {
    ""autoUpdateIntervalInMinutes"": 5
  }
}";
            var userSettingsFile = this.SetUpSettingsFile(existingContentFile);

            var inputSettings = new Hashtable()
            {
                {
                    "source",
                    new Hashtable()
                    {
                        {
                            "autoUpdateIntervalInMinutes",
                            5
                        },
                    }
                },
                {
                    "visual",
                    new Hashtable()
                    {
                        {
                            "progressBar",
                            "rainbow"
                        },
                    }
                },
                {
                    "installBehavior",
                    new Hashtable()
                    {
                        {
                            "preferences",
                            new Hashtable()
                            {
                                {
                                    "scope",
                                    "user"
                                },
                                {
                                    "locale",
                                    new string[] { "en-US", "es-MX" }
                                },
                            }
                        },
                    }
                },
                {
                    "telemetry",
                    new Hashtable()
                    {
                        {
                            "disable",
                            false
                        },
                    }
                },
            };

            var userSettings = new UserSettings(inputSettings, userSettingsFile, true);

            Assert.False(userSettings.Test());
        }

        /// <summary>
        /// Tests UserSettings Test method Overwrite. Settings are not equal. Existing settings contains more elements.
        /// </summary>
        [Fact]
        public void UserSettings_Test_Overwrite_NotEqual_MoreSettingsProperties()
        {
            string existingContentFile = @"{
  ""$schema"": ""https://aka.ms/winget-settings.schema.json"",
  ""installBehavior"": {
    ""preferences"": {
      ""scope"": ""user"",
      ""locale"": [
        ""en-US"",
        ""es-MX"",
      ]
    }
  },
  ""visual"": {
    ""progressBar"": ""rainbow""
  },
  ""source"": {
    ""autoUpdateIntervalInMinutes"": 5
  }
}";
            var userSettingsFile = this.SetUpSettingsFile(existingContentFile);

            var inputSettings = new Hashtable()
            {
                {
                    "source",
                    new Hashtable()
                    {
                        {
                            "autoUpdateIntervalInMinutes",
                            5
                        },
                    }
                },
                {
                    "visual",
                    new Hashtable()
                    {
                        {
                            "progressBar",
                            "rainbow"
                        },
                    }
                },
                {
                    "installBehavior",
                    new Hashtable()
                    {
                        {
                            "preferences",
                            new Hashtable()
                            {
                                {
                                    "locale",
                                    new string[] { "en-US", "es-MX" }
                                },
                            }
                        },
                    }
                },
                {
                    "telemetry",
                    new Hashtable()
                    {
                        {
                            "disable",
                            false
                        },
                    }
                },
            };

            var userSettings = new UserSettings(inputSettings, userSettingsFile, true);

            Assert.False(userSettings.Test());
        }

        /// <summary>
        /// Tests UserSettings Test method Overwrite. Settings are equal. Existing has no schema element, input does.
        /// </summary>
        [Fact]
        public void UserSettings_Test_Overwrite_Equal_ExistingNoSchema()
        {
            string existingContentFile = @"{
  ""source"": {
    ""autoUpdateIntervalInMinutes"": 5
  }
}";
            var userSettingsFile = this.SetUpSettingsFile(existingContentFile);

            var inputSettings = new Hashtable()
            {
                {
                    "$schema",
                    "https://aka.ms/winget-settings.schema.json"
                },
                {
                    "source",
                    new Hashtable()
                    {
                        {
                            "autoUpdateIntervalInMinutes",
                            5
                        },
                    }
                },
            };

            var userSettings = new UserSettings(inputSettings, userSettingsFile, true);

            Assert.True(userSettings.Test());
        }

        /// <summary>
        /// Tests UserSettings Test method Overwrite. Settings are equal. Existing has schema element, input does not.
        /// </summary>
        [Fact]
        public void UserSettings_Test_Overwrite_Equal_InputNoSchema()
        {
            string existingContentFile = @"{
  ""$schema"": ""https://aka.ms/winget-settings.schema.json"",
  ""installBehavior"": {
    ""preferences"": {
      ""scope"": ""user"",
      ""locale"": [
        ""en-US"",
        ""es-MX"",
      ]
    }
  },
  ""telemetry"": {
    ""disable"": false
  },
  ""visual"": {
    ""progressBar"": ""rainbow""
  },
  ""source"": {
    ""autoUpdateIntervalInMinutes"": 5
  }
}";
            var userSettingsFile = this.SetUpSettingsFile(existingContentFile);

            var inputSettings = new Hashtable()
            {
                {
                    "source",
                    new Hashtable()
                    {
                        {
                            "autoUpdateIntervalInMinutes",
                            5
                        },
                    }
                },
                {
                    "visual",
                    new Hashtable()
                    {
                        {
                            "progressBar",
                            "rainbow"
                        },
                    }
                },
                {
                    "installBehavior",
                    new Hashtable()
                    {
                        {
                            "preferences",
                            new Hashtable()
                            {
                                {
                                    "scope",
                                    "user"
                                },
                                {
                                    "locale",
                                    new string[] { "en-US", "es-MX" }
                                },
                            }
                        },
                    }
                },
                {
                    "telemetry",
                    new Hashtable()
                    {
                        {
                            "disable",
                            false
                        },
                    }
                },
            };

            var userSettings = new UserSettings(inputSettings, userSettingsFile, true);

            Assert.True(userSettings.Test());
        }

        /// <summary>
        /// Tests UserSettings Test method Overwrite. Settings are equal.
        /// </summary>
        [Fact]
        public void UserSettings_Test_Overwrite_NotEqual_DifferentValues()
        {
            string existingContentFile = @"{
  ""installBehavior"": {
    ""preferences"": {
      ""scope"": ""user"",
      ""locale"": [
        ""en-US"",
        ""es-MX"",
      ]
    }
  },
  ""telemetry"": {
    ""disable"": false
  },
  ""visual"": {
    ""progressBar"": ""rainbow""
  },
  ""source"": {
    ""autoUpdateIntervalInMinutes"": 5
  }
}";
            var userSettingsFile = this.SetUpSettingsFile(existingContentFile);

            var inputSettings = new Hashtable()
            {
                {
                    "source",
                    new Hashtable()
                    {
                        {
                            "autoUpdateIntervalInMinutes",
                            5
                        },
                    }
                },
                {
                    "visual",
                    new Hashtable()
                    {
                        {
                            "progressBar",
                            "retro"
                        },
                    }
                },
                {
                    "installBehavior",
                    new Hashtable()
                    {
                        {
                            "preferences",
                            new Hashtable()
                            {
                                {
                                    "scope",
                                    "user"
                                },
                                {
                                    "locale",
                                    new string[] { "en-US", "es-MX" }
                                },
                            }
                        },
                    }
                },
                {
                    "telemetry",
                    new Hashtable()
                    {
                        {
                            "disable",
                            false
                        },
                    }
                },
            };

            var userSettings = new UserSettings(inputSettings, userSettingsFile, true);

            Assert.False(userSettings.Test());
        }

        /// <summary>
        /// UserSettings Test method Partial. Input is in settings.
        /// </summary>
        [Fact]
        public void UserSettings_Test_Partial_Equal_ContainsAll()
        {
            string existingContentFile = @"{
  ""installBehavior"": {
    ""preferences"": {
      ""scope"": ""user"",
      ""locale"": [
        ""en-US"",
        ""es-MX"",
      ]
    }
  },
  ""telemetry"": {
    ""disable"": false
  },
  ""visual"": {
    ""progressBar"": ""retro""
  },
  ""source"": {
    ""autoUpdateIntervalInMinutes"": 5
  }
}";
            var userSettingsFile = this.SetUpSettingsFile(existingContentFile);

            var inputSettings = new Hashtable()
            {
                {
                    "source",
                    new Hashtable()
                    {
                        {
                            "autoUpdateIntervalInMinutes",
                            5
                        },
                    }
                },
                {
                    "visual",
                    new Hashtable()
                    {
                        {
                            "progressBar",
                            "retro"
                        },
                    }
                },
                {
                    "installBehavior",
                    new Hashtable()
                    {
                        {
                            "preferences",
                            new Hashtable()
                            {
                                {
                                    "scope",
                                    "user"
                                },
                                {
                                    "locale",
                                    new string[] { "en-US", "es-MX" }
                                },
                            }
                        },
                    }
                },
                {
                    "telemetry",
                    new Hashtable()
                    {
                        {
                            "disable",
                            false
                        },
                    }
                },
            };

            var userSettings = new UserSettings(inputSettings, userSettingsFile, true);

            Assert.True(userSettings.Test());
        }

        /// <summary>
        /// UserSettings Test method Partial. Input is in settings.
        /// </summary>
        [Fact]
        public void UserSettings_Test_Partial_Equal_InputContainsLess()
        {
            string existingContentFile = @"{
  ""installBehavior"": {
    ""preferences"": {
      ""scope"": ""user"",
      ""locale"": [
        ""en-US"",
        ""es-MX"",
      ]
    }
  },
  ""telemetry"": {
    ""disable"": false
  },
  ""visual"": {
    ""progressBar"": ""rainbow""
  },
  ""source"": {
    ""autoUpdateIntervalInMinutes"": 5
  }
}";
            var userSettingsFile = this.SetUpSettingsFile(existingContentFile);

            var inputSettings = new Hashtable()
            {
                {
                    "source",
                    new Hashtable()
                    {
                        {
                            "autoUpdateIntervalInMinutes",
                            5
                        },
                    }
                },
                {
                    "installBehavior",
                    new Hashtable()
                    {
                        {
                            "preferences",
                            new Hashtable()
                            {
                                {
                                    "scope",
                                    "user"
                                },
                                {
                                    "locale",
                                    new string[] { "en-US", "es-MX" }
                                },
                            }
                        },
                    }
                },
                {
                    "telemetry",
                    new Hashtable()
                    {
                        {
                            "disable",
                            false
                        },
                    }
                },
            };

            var userSettings = new UserSettings(inputSettings, userSettingsFile, false);

            Assert.True(userSettings.Test());
        }

        /// <summary>
        /// UserSettings Test method Partial. Input is not in the settings because it has more properties.
        /// </summary>
        [Fact]
        public void UserSettings_Test_Partial_NotEqual_InputContainsMore()
        {
            string existingContentFile = @"{
  ""installBehavior"": {
    ""preferences"": {
      ""scope"": ""user"",
      ""locale"": [
        ""en-US"",
        ""es-MX"",
      ]
    }
  },
  ""visual"": {
    ""progressBar"": ""retro""
  },
  ""source"": {
    ""autoUpdateIntervalInMinutes"": 5
  }
}";
            var userSettingsFile = this.SetUpSettingsFile(existingContentFile);

            var inputSettings = new Hashtable()
            {
                {
                    "source",
                    new Hashtable()
                    {
                        {
                            "autoUpdateIntervalInMinutes",
                            5
                        },
                    }
                },
                {
                    "visual",
                    new Hashtable()
                    {
                        {
                            "progressBar",
                            "retro"
                        },
                    }
                },
                {
                    "installBehavior",
                    new Hashtable()
                    {
                        {
                            "preferences",
                            new Hashtable()
                            {
                                {
                                    "scope",
                                    "user"
                                },
                                {
                                    "locale",
                                    new string[] { "en-US", "es-MX" }
                                },
                            }
                        },
                    }
                },
                {
                    "telemetry",
                    new Hashtable()
                    {
                        {
                            "disable",
                            false
                        },
                    }
                },
            };

            var userSettings = new UserSettings(inputSettings, userSettingsFile, false);

            Assert.False(userSettings.Test());
        }

        /// <summary>
        /// UserSettings Test method Partial. Input is not in the settings because a property has a different value.
        /// </summary>
        [Fact]
        public void UserSettings_Test_Partial_NotEqual_DifferentValues()
        {
            string existingContentFile = @"{
  ""installBehavior"": {
    ""preferences"": {
      ""scope"": ""user"",
      ""locale"": [
        ""en-US"",
        ""es-MX"",
      ]
    }
  },
  ""telemetry"": {
    ""disable"": false
  },
  ""visual"": {
    ""progressBar"": ""rainbow""
  },
  ""source"": {
    ""autoUpdateIntervalInMinutes"": 5
  }
}";
            var userSettingsFile = this.SetUpSettingsFile(existingContentFile);

            var inputSettings = new Hashtable()
            {
                {
                    "source",
                    new Hashtable()
                    {
                        {
                            "autoUpdateIntervalInMinutes",
                            5
                        },
                    }
                },
                {
                    "visual",
                    new Hashtable()
                    {
                        {
                            "progressBar",
                            "retro"
                        },
                    }
                },
                {
                    "installBehavior",
                    new Hashtable()
                    {
                        {
                            "preferences",
                            new Hashtable()
                            {
                                {
                                    "scope",
                                    "user"
                                },
                                {
                                    "locale",
                                    new string[] { "en-US", "es-MX" }
                                },
                            }
                        },
                    }
                },
                {
                    "telemetry",
                    new Hashtable()
                    {
                        {
                            "disable",
                            false
                        },
                    }
                },
            };

            var userSettings = new UserSettings(inputSettings, userSettingsFile, false);

            Assert.False(userSettings.Test());
        }

        /// <summary>
        /// Tests user settings Get. Gets the user settings then calls test to make sure it is right.
        /// </summary>
        [Fact]
        public void UserSettings_Get()
        {
            string existingContentFile = @"{
  ""installBehavior"": {
    ""preferences"": {
      ""scope"": ""user"",
      ""locale"": [
        ""en-US"",
        ""es-MX"",
      ]
    }
  },
  ""telemetry"": {
    ""disable"": false
  },
  ""visual"": {
    ""progressBar"": ""rainbow""
  },
  ""source"": {
    ""autoUpdateIntervalInMinutes"": 5
  }
}";
            var userSettingsFile = this.SetUpSettingsFile(existingContentFile);

            var userSettings = new UserSettings(userSettingsFile, true);

            var newUserSettings = userSettings.Get();

            Assert.True(newUserSettings.Test());
        }

        private string SetUpSettingsFile(string existingContentFile)
        {
            var userSettingsFile = Path.Combine(this.mockFileDirectory, Path.GetRandomFileName());
            File.WriteAllText(userSettingsFile, existingContentFile);
            return userSettingsFile;
        }
    }
}
