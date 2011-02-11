using System;

namespace KeePass.Utils
{
    internal static class TrialManager
    {
        private static bool? _isTrial;

        private static bool _popupShown;

        public static bool IsTrial
        {
            get
            {
                if (_isTrial == null)
                {
#if DEBUG || FREEWARE
                    _isTrial = true;
#else
                    _isTrial = new Microsoft.Phone.Marketplace
                        .LicenseInformation().IsTrial();
#endif
                }

                return _isTrial.Value;
            }
        }

        public static void PopupShown()
        {
            _popupShown = true;
        }

        public static bool ShouldShowPopup()
        {
            return IsTrial && !_popupShown;
        }
    }
}