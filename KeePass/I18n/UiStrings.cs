using System;

namespace KeePass.I18n
{
    public class UiStrings
    {
        private readonly Strings _strings;

        public Strings Strings
        {
            get { return _strings; }
        }

        public UiStrings()
        {
            _strings = new Strings();
        }
    }
}