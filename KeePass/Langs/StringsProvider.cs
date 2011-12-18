using System;

namespace KeePass.Langs
{
    public class StringsProvider
    {
        private readonly About _about;
        private readonly AnalyticsSettings _analytics;
        private readonly EntryDetails _entry;
        private readonly App _resources;

        public About About
        {
            get { return _about; }
        }

        public AnalyticsSettings Analytics
        {
            get { return _analytics; }
        }

        public App App
        {
            get { return _resources; }
        }

        public EntryDetails Entry
        {
            get { return _entry; }
        }

        public StringsProvider()
        {
            _about = new About();
            _resources = new App();
            _entry = new EntryDetails();
            _analytics = new AnalyticsSettings();
        }
    }
}