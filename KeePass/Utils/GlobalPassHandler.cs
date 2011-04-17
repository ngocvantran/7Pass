using System;
using KeePass.IO.Utils;

namespace KeePass.Utils
{
    internal class GlobalPassHandler
    {
        private readonly AppSettings _settings;
        private bool _globalPassEntered;

        /// <summary>
        /// Gets a value indicating whether 7Pass is password protected.
        /// </summary>
        /// <value><c>true</c> 7Pass is password
        /// protected; otherwise, <c>false</c>.
        /// </value>
        public bool HasGlobalPass
        {
            get
            {
                return !string.IsNullOrEmpty(
                    _settings.Password);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the glo.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [should prompt global pass]; otherwise, <c>false</c>.
        /// </value>
        public bool ShouldPromptGlobalPass
        {
            get
            {
                return HasGlobalPass &&
                    !_globalPassEntered;
            }
        }

        public GlobalPassHandler(AppSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");

            _settings = settings;
        }

        /// <summary>
        /// Notifies that the user has successfully entered the global password.
        /// </summary>
        public void GloablPassEntered()
        {
            _globalPassEntered = true;
        }

        /// <summary>
        /// Verifies if the specified global password is correct.
        /// </summary>
        /// <param name="password">The global password.</param>
        /// <returns></returns>
        public bool Verify(string password)
        {
            var hash = BufferEx.GetHash(password);
            if (hash != _settings.Password)
                return false;

            _globalPassEntered = true;
            return true;
        }
    }
}