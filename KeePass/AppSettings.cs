using System;
using System.Runtime.Serialization;

namespace KeePass
{
    [DataContract]
    public class AppSettings
    {
        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>The password.</value>
        [DataMember]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the password should be stored..
        /// </summary>
        /// <value><c>true</c> if the password should be stored; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool StorePassword { get; set; }
    }
}