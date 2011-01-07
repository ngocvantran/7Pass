using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using KeePass.IO.Utils;

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
        private readonly string _url;

        /// <summary>
        /// Gets or sets the histories.
        /// </summary>
        /// <value>The histories.</value>
        public List<Entry> Histories { get; set; }

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
        /// Gets the value of the specified key.
        /// </summary>
        /// <value>Value of the specified key.</value>
        public string this[string key]
        {
            get { return _fields[key]; }
        }

        /// <summary>
        /// Gets or sets the last modified time.
        /// </summary>
        /// <value>The last modified time.</value>
        public DateTime LastModified { get; set; }

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
        /// Gets the Url.
        /// </summary>
        /// <value>The Url.</value>
        public string Url
        {
            get { return _url; }
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
            _url = GetUrl();
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

        private string GetUrl()
        {
            var url = TryGet(KEY_URL);

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

        private string TryGet(string key)
        {
            string value;
            return _fields.TryGetValue(key, out value)
                ? value : null;
        }
    }
}