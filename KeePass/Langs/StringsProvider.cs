using System;

namespace KeePass.Langs
{
    public class StringsProvider
    {
        private readonly About _about;
        private readonly Strings _resources;

        public About About
        {
            get { return _about; }
        }

        public Strings Resources
        {
            get { return _resources; }
        }

        public StringsProvider()
        {
            _resources = new Strings();
            _about = new About();
        }
    }
}