using System;

namespace KeePass.Langs
{
    public class StringsProvider
    {
        private readonly About _about;
        private readonly AnalyticsSettings _analytics;
        private readonly Strings _resources;

        public About About
        {
            get { return _about; }
        }

        public AnalyticsSettings Analytics
        {
            get { return _analytics; }
        }

        public Strings Resources
        {
            get { return _resources; }
        }

        public StringsProvider()
        {
            _resources = new Strings();
            _about = new About();
            _analytics = new AnalyticsSettings();
        }
    }
}