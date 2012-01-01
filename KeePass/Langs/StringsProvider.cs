using System;

namespace KeePass.Langs
{
    public class StringsProvider
    {
        private readonly About _about;
        private readonly AnalyticsSettings _analytics;
        private readonly Entry _entry;
        private readonly Group _group;
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

        public Entry Entry
        {
            get { return _entry; }
        }

        public Group Group
        {
            get { return _group; }
        }

        public StringsProvider()
        {
            _about = new About();
            _entry = new Entry();
            _resources = new App();
            _group = new Group();
            _analytics = new AnalyticsSettings();
        }
    }
}