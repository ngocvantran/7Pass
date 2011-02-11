using System;
using System.IO.IsolatedStorage;

namespace KeePass.Utils
{
    internal class AppSettings
    {
        private const string KEY_HIDE_BIN = "HideRecycleBin";
        private const string KEY_USE_INT_BROWSER = "UseIntegratedBrowser";

        private static AppSettings _instance;
        private readonly IsolatedStorageSettings _settings;

        /// <summary>
        /// Gets or sets a value indicating
        /// whether the recycle bin is hidden.
        /// </summary>
        /// <value>
        /// <c>true</c> if the recycle bin
        /// is hidden; otherwise, <c>false</c>.
        /// </value>
        public bool HideRecycleBin
        {
            get { return "1" == this[KEY_HIDE_BIN]; }
            set { this[KEY_HIDE_BIN] = value ? "1" : "0"; }
        }

        /// <summary>
        /// Gets the cached instance.
        /// </summary>
        public static AppSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AppSettings(
                        IsolatedStorageSettings.ApplicationSettings);
                }

                return _instance;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the
        /// integrated browser should be used when user tap the URL.
        /// </summary>
        /// <value>
        /// <c>true</c> if the integrated browser should
        /// be used when user tap the URL; otherwise, <c>false</c>.
        /// </value>
        public bool UseIntBrowser
        {
            get
            {
                var value = this[KEY_USE_INT_BROWSER];
                return value == null || value == "1";
            }
            set { this[KEY_USE_INT_BROWSER] = value ? "1" : "0"; }
        }

        private string this[string key]
        {
            get
            {
                string value;
                if (!_settings.TryGetValue(key, out value))
                    value = null;

                return value;
            }
            set
            {
                string current;
                if (!_settings.TryGetValue(key, out current))
                    _settings.Add(key, value);
                else if (value != current)
                    _settings[key] = value;
                else
                    return;

                _settings.Save();
            }
        }

        public AppSettings(IsolatedStorageSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");

            _settings = settings;
        }
    }
}