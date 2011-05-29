using System;

namespace KeePass.IO.Data
{
    public class DbPersistentData
    {
        /// <summary>
        /// Gets or sets the protection.
        /// </summary>
        /// <value>The protection.</value>
        public byte[] Protection { get; set; }

        /// <summary>
        /// Gets or sets the XML.
        /// </summary>
        /// <value>The XML.</value>
        public byte[] Xml { get; set; }
    }
}