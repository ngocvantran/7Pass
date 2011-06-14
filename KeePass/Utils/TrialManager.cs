using System;
using System.Windows;
using System.Windows.Controls;
using Coding4Fun.Phone.Controls;
using Microsoft.Phone.Tasks;

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

        public static void CheckToastState()
        {
            if (!ShouldShowPopup())
                return;

            _popupShown = true;
            var toast = new ToastPrompt
            {
                TextOrientation = Orientation
                    .Horizontal,
            };

            toast.Loaded += toast_Loaded;
            toast.Completed += toast_Completed;

#if !FREEWARE
            toast.Message = Properties.Resources.ToastFree;
#else
            toast.Message = Properties.Resources.ToastTrial;
#endif
            
            toast.Show();
        }

        private static void toast_Loaded(
            object sender, RoutedEventArgs e)
        {
            AppSettings.Instance.ToastShowns++;
        }

        private static void toast_Completed(object sender,
            PopUpEventArgs<string, PopUpResult> e)
        {
            if (e.PopUpResult == PopUpResult.Ok)
                ShowMarketPlace();
        }

        private static bool ShouldShowPopup()
        {
            return IsTrial && !_popupShown &&
                AppSettings.Instance.ToastShowns < 5;
        }

        private static void ShowMarketPlace()
        {
#if !FREEWARE
            new MarketplaceDetailTask().Show();
#else
            new MarketplaceDetailTask
            {
                ContentType = MarketplaceContentType.Applications,
                ContentIdentifier = "2f1d7d92-cef4-df11-9264-00237de2db9e",
            }.Show();
#endif
        }
    }
}