using System;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Tasks;

namespace KeePass.Services
{
    public static class TrialManager
    {
        public const int USAGES_LIMIT = 50;

        private static readonly bool _isTrial;
        private static readonly IsolatedStorageSettings _settings;

        public static bool HasExpired
        {
            get { return !DemoDb && Usages > USAGES_LIMIT; }
        }

        public static bool IsTrial
        {
            get { return _isTrial; }
        }

        public static int Usages
        {
            get
            {
                int usages;
                return _settings.TryGetValue(
                    Consts.KEY_USAGES, out usages)
                    ? usages : 0;
            }
            private set
            {
                _settings[Consts.KEY_USAGES] = value;
                _settings.Save();
            }
        }

        private static bool DemoDb
        {
            get
            {
                bool demoDb;
                return _settings.TryGetValue(
                    Consts.KEY_DEMO_DB, out demoDb)
                        && demoDb;
            }
            set
            {
                _settings[Consts.KEY_DEMO_DB] = value;
                _settings.Save();
            }
        }

        static TrialManager()
        {
#if DEBUG
            _isTrial = true;
#else
            _isTrial = new Microsoft.Phone.Marketplace
                .LicenseInformation().IsTrial();
#endif

            _settings = IsolatedStorageSettings
                .ApplicationSettings;
        }

        public static void ShowPurchase()
        {
            new MarketplaceDetailTask().Show();
        }

        public static void UpdateUsages()
        {
            if (_isTrial && !DemoDb)
                Usages++;
        }

        public static void UseDemoDb()
        {
            if (!DemoDb)
                DemoDb = true;
        }

        public static void UpdatedDb()
        {
            Usages = DemoDb ? 0 : 1;
        }

        public static void UseRealDb()
        {
            if (DemoDb)
                Usages++;

            DemoDb = false;
        }
    }
}