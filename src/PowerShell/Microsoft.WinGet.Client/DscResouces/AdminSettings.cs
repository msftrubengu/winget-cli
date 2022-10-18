// -----------------------------------------------------------------------------
// <copyright file="AdminSettings.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. Licensed under the MIT License.
// </copyright>
// -----------------------------------------------------------------------------

namespace Microsoft.WinGet.Client.DscResouces
{
    /// <summary>
    /// WinGet admin settings DSC Resource.
    /// </summary>
    public class AdminSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdminSettings"/> class.
        /// </summary>
        public AdminSettings()
        {
        }

        /// <summary>
        /// Retrieves the state of the resource the admin settings resource.
        /// </summary>
        /// <returns>AdminSettings.</returns>
        public AdminSettings Get()
        {
            return new AdminSettings();
        }

        /// <summary>
        /// Determines if the admin settings node is currently compliant with the resource's desired state.
        /// </summary>
        /// <returns>True if compliant.</returns>
        public bool Test()
        {
            return false;
        }

        /// <summary>
        /// Attempts to force the set node to become compliant with the resource's desired state.
        /// </summary>
        public void Set()
        {
        }
    }
}
