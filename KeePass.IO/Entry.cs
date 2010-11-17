using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace KeePass.IO
{
    [DebuggerDisplay("Entry {Title}")]
    public class Entry
    {
        private const string NOTES = "Notes";
        private const string PASS = "Password";
        private const string TITLE = "Title";
        private const string USER = "UserName";

        private static readonly string[] _known = new[] {USER, PASS, NOTES, TITLE};

        private readonly IDictionary<string, string> _fields;

        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        /// <value>The ID.</value>
        public Guid ID { get; set; }

        /// <summary>
        /// Gets the notes.
        /// </summary>
        /// <value>The notes.</value>
        public string Notes
        {
            get { return TryGet(NOTES); }
        }

        /// <summary>
        /// Gets the password.
        /// </summary>
        /// <value>The password.</value>
        public string Password
        {
            get { return TryGet(PASS); }
        }

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <value>The title.</value>
        public string Title
        {
            get { return TryGet(TITLE); }
        }

        /// <summary>
        /// Gets the name of the user.
        /// </summary>
        /// <value>The name of the user.</value>
        public string UserName
        {
            get { return TryGet(USER); }
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
                .Select(x => _fields[x])
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