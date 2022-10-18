// -----------------------------------------------------------------------------
// <copyright file="Install.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. Licensed under the MIT License.
// </copyright>
// -----------------------------------------------------------------------------

namespace Microsoft.WinGet.Client.DscResouces
{
    /// <summary>
    /// WinGet install DSC Resource.
    /// </summary>
    public class Install
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Install"/> class.
        /// </summary>
        public Install()
        {
        }

        /// <summary>
        /// Retrieves the state of the resource the install resource.
        /// </summary>
        /// <returns>Install.</returns>
        public Install Get()
        {
            return new Install();
        }

        /// <summary>
        /// Determines if the install node is currently compliant with the resource's desired state.
        /// </summary>
        /// <returns>True if compliant.</returns>
        public bool Test()
        {
            return false;
        }

        /// <summary>
        /// Attempts to force the install node to become compliant with the resource's desired state.
        /// </summary>
        public void Set()
        {
        }
    }
}
