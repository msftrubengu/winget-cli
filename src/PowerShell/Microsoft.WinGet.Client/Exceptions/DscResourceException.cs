// -----------------------------------------------------------------------------
// <copyright file="DscResourceException.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. Licensed under the MIT License.
// </copyright>
// -----------------------------------------------------------------------------

namespace Microsoft.WinGet.Client.Exceptions
{
    using System;

    /// <summary>
    /// WinGet DscResourceException.
    /// </summary>
    public class DscResourceException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DscResourceException"/> class.
        /// </summary>
        public DscResourceException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DscResourceException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public DscResourceException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DscResourceException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="inner">Inner exception.</param>
        public DscResourceException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
