using System;
using System.Windows;
using System.Windows.Controls;

namespace KeePass.Generator
{
    internal class CharacterSetCheckBox : CheckBox
    {
        private readonly ICharacterSet _set;

        /// <summary>
        /// Gets the characters.
        /// </summary>
        public char[] Characters
        {
            get { return _set.Charaters; }
        }

        public CharacterSetCheckBox(ICharacterSet set)
        {
            if (set == null)
                throw new ArgumentNullException("set");

            _set = set;
            
            MinWidth = 300;
            Content = set.Name;
            Margin = new Thickness(0);
        }

        /// <summary>
        /// Loads the state.
        /// </summary>
        /// <param name="password">The password.</param>
        public void LoadState(string password)
        {
            var index = password.IndexOfAny(
                _set.Charaters);

            IsChecked = index >= 0;
        }
    }
}