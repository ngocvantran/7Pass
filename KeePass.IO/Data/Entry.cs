using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using KeePass.IO.Utils;

namespace KeePass.IO.Data
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
        private readonly Dictionary<string, string> _original;

        /// <summary>
        /// Gets the custom fields.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, string> CustomFields
        {
            get
            {
                return _fields.Keys.Except(_known)
                    .ToDictionary(x => x, x => _fields[x]);
            }
        }

        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        public Group Group { get; set; }

        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        /// <value>The ID.</value>
        public string ID { get; set; }

        /// <summary>
        /// Gets or sets the icon data.
        /// </summary>
        /// <value>
        /// The icon data.
        /// </value>
        public IconData Icon { get; set; }

        /// <summary>
        /// Gets or sets the value of the specified key.
        /// </summary>
        /// <value>Value of the specified key.</value>
        public string this[string key]
        {
            get { return TryGet(key); }
            set { _fields.AddOrSet(key, value); }
        }

        /// <summary>
        /// Gets or sets the last modified time.
        /// </summary>
        /// <value>
        /// The last modified time.
        /// </value>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Gets or sets the notes.
        /// </summary>
        /// <value>The notes.</value>
        public string Notes
        {
            get { return this[KEY_NOTES]; }
            set { this[KEY_NOTES] = value; }
        }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>The password.</value>
        public string Password
        {
            get { return this[KEY_PASS]; }
            set { this[KEY_PASS] = value; }
        }

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <value>The title.</value>
        public string Title
        {
            get { return this[KEY_TITLE]; }
            set { this[KEY_TITLE] = value; }
        }

        /// <summary>
        /// Gets the Url.
        /// </summary>
        /// <value>The Url.</value>
        public string Url
        {
            get { return this[KEY_URL]; }
            set { this[KEY_URL] = value; }
        }

        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        /// <value>The name of the user.</value>
        public string UserName
        {
            get { return this[KEY_USER]; }
            set { this[KEY_USER] = value; }
        }

        public Entry(IDictionary<string, string> fields)
        {
            if (fields == null)
                throw new ArgumentNullException("fields");

            Icon = new IconData();

            _fields = fields;
            _original = new Dictionary<string, string>(fields);
        }

        public Entry()
            : this(new Dictionary<string, string>()) {}

        /// <summary>
        /// Gets all fields.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, string> GetAllFields()
        {
            return new Dictionary<string, string>(_fields);
        }

        /// <summary>
        /// Gets the navigatable URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>Navigatable URL.</returns>
        public string GetNavigateUrl(string url)
        {
            if (url == null)
                return null;

            var keys = _fields.Keys.ToList();
            keys.Remove(KEY_URL);

            foreach (var key in keys)
            {
                var pattern = GetPattern(key);

                url = StringReplace.Replace(url,
                    pattern, _fields[key], StringComparison
                        .InvariantCultureIgnoreCase);
            }

            return url;
        }

        /// <summary>
        /// Removes this entry from its group.
        /// </summary>
        public void Remove()
        {
            Group.Entries
                .Remove(this);
        }

        /// <summary>
        /// Resets this entry.
        /// </summary>
        public void Reset()
        {
            _fields.Clear();

            foreach (var pair in _original)
                _fields.Add(pair.Key, pair.Value);
        }

        private static string GetPattern(string key)
        {
            foreach (var known in _known)
            {
                if (string.Equals(key, known,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    return "{" + known + "}";
                }
            }

            return "{S:" + key + "}";
        }

        private string TryGet(string key)
        {
            string value;
            return _fields.TryGetValue(key, out value)
                ? value : null;
        }
    }
}