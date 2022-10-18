// -----------------------------------------------------------------------------
// <copyright file="Source.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. Licensed under the MIT License.
// </copyright>
// -----------------------------------------------------------------------------

namespace Microsoft.WinGet.Client.DscResouces
{
    /// <summary>
    /// WinGet source DSC Resource.
    /// </summary>
    public class Source
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Source"/> class.
        /// </summary>
        public Source()
        {
        }

        /// <summary>
        /// Retrieves the state of the resource the source resource.
        /// </summary>
        /// <returns>Source.</returns>
        public Source Get()
        {
            return new Source();
        }

        /// <summary>
        /// Determines if the source node is currently compliant with the resource's desired state.
        /// </summary>
        /// <returns>True if compliant.</returns>
        public bool Test()
        {
            return false;
        }

        /// <summary>
        /// Attempts to force the source node to become compliant with the resource's desired state.
        /// </summary>
        public void Set()
        {
        }
    }
}
