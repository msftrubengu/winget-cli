// -----------------------------------------------------------------------------
// <copyright file="BaseCommand.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. Licensed under the MIT License.
// </copyright>
// -----------------------------------------------------------------------------

namespace Microsoft.WinGet.Client.Common
{
    using System;
    using System.Management.Automation;

    /// <summary>
    /// Base class for all Cmdlets.
    /// </summary>
    public abstract class BaseCommand : PSCmdlet
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCommand"/> class.
        /// </summary>
        public BaseCommand()
            : base()
        {
            // TODO: fix to system when Ryan's change is merged.
            if (Utilities.ExecutingAsAdministrator)
            {
                throw new Exception(Utilities.ResourceManager.GetString("ExceptionAdministratorDisabled"));
            }
        }
    }
}
