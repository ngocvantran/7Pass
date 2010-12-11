using System;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Marketplace;

namespace KeePass.Services
{
    public static class TrialManager
    {
        public const int USAGES_LIMIT = 50;

        private static readonly bool _isTrial;
        private static readonly IsolatedStorageSettings _settings;

        public static bool HasExpired
        {
            get { return Usages > USAGES_LIMIT; }
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

        static TrialManager()
        {
#if DEBUG
            _isTrial = true;
#else
            _isTrial = new LicenseInformation().IsTrial();
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
            if (_isTrial)
                Usages++;
        }
    }
}