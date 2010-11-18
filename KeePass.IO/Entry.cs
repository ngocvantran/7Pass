using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace KeePass.IO
{
    [DebuggerDisplay("Entry {Title}")]
    public class Entry
    {
        private const string KEY_NOTES = "Notes";
        private const string KEY_PASS = "Password";
        private const string KEY_TITLE = "Title";
        private const string KEY_URL = "URL";
        private const string KEY_USER = "UserName";

        private static readonly string[] _known = new[]
        {
            KEY_USER, KEY_PASS, KEY_NOTES, KEY_TITLE, KEY_URL
        };

        private readonly IDictionary<string, string> _fields;

        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        /// <value>The ID.</value>
        public Guid ID { get; set; }

        /// <summary>
        /// Gets the value of the specified key.
        /// </summary>
        /// <value>Value of the specified key.</value>
        public string this[string key]
        {
            get { return _fields[key]; }
        }

        /// <summary>
        /// Gets the notes.
        /// </summary>
        /// <value>The notes.</value>
        public string Notes
        {
            get { return TryGet(KEY_NOTES); }
        }

        /// <summary>
        /// Gets the password.
        /// </summary>
        /// <value>The password.</value>
        public string Password
        {
            get { return TryGet(KEY_PASS); }
        }

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <value>The title.</value>
        public string Title
        {
            get { return TryGet(KEY_TITLE); }
        }

        /// <summary>
        /// Gets the U rl.
        /// </summary>
        /// <value>The U rl.</value>
        public string Url
        {
            get { return TryGet(KEY_URL); }
        }

        /// <summary>
        /// Gets the name of the user.
        /// </summary>
        /// <value>The name of the user.</value>
        public string UserName
        {
            get { return TryGet(KEY_USER); }
        }

        public Entry(IDictionary<string, string> fields)
        {
            if (fields == null)
                throw new ArgumentNullException("fields");
            _fields = fields;
        }

        public Entry()
            : this(new Dictionary<string, string>()) {}

        /// <summary>
        /// Gets other fields.
        /// </summary>
        /// <returns>Other fields</returns>
        public string[] GetOthers()
        {
            return _fields.Keys
                .Except(_known)
                .ToArray();
        }

        private string TryGet(string key)
        {
            string value;
            return _fields.TryGetValue(key, out value)
                ? value : null;
        }
    }
}