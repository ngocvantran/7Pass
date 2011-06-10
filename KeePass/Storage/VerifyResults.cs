using System;

namespace KeePass.Storage
{
    internal class VerifyResults
    {
        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        /// <value>
        /// The result.
        /// </value>
        public VerifyResultTypes Result { get; set; }
    }
}