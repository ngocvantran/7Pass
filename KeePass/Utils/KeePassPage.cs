using System;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;

namespace KeePass.Utils
{
    public class KeePassPage : PhoneApplicationPage
    {
        public KeePassPage()
        {
            SupportedOrientations = SupportedPageOrientation
                .PortraitOrLandscape;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var globalPass = AppSettings
                .Instance.GlobalPass;

            if (!globalPass.ShouldPromptGlobalPass)
            {
                OnNavigatedTo(false, e);
                return;
            }

            OnNavigatedTo(true, e);
            this.NavigateTo<GlobalPassVerify>();
        }

        protected virtual void OnNavigatedTo(
            bool cancelled, NavigationEventArgs e) {}
    }
}