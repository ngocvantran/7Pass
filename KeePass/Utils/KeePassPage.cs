using System;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace KeePass.Utils
{
    public class KeePassPage : PhoneApplicationPage
    {
        public KeePassPage()
        {
            SupportedOrientations = SupportedPageOrientation
                .PortraitOrLandscape;
        }

        protected ApplicationBarIconButton AppButton(int index)
        {
            return (ApplicationBarIconButton)
                ApplicationBar.Buttons[index];
        }

        protected ApplicationBarMenuItem AppMenuItem(int index)
        {
            return (ApplicationBarMenuItem)
                ApplicationBar.MenuItems[index];
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var globalPass = AppSettings
                .Instance.GlobalPass;

            if (!globalPass.ShouldPromptGlobalPass)
            {
                SetTransition();
                OnNavigatedTo(false, e);

                return;
            }

            OnNavigatedTo(true, e);
            this.NavigateTo<GlobalPassVerify>();
        }

        protected virtual void OnNavigatedTo(
            bool cancelled, NavigationEventArgs e) {}

        private void SetTransition()
        {
            TransitionService.SetNavigationInTransition(
                this, new NavigationInTransition
                {
                    Backward = new TurnstileTransition
                    {
                        Mode = TurnstileTransitionMode.BackwardIn,
                    },
                    Forward = new TurnstileTransition
                    {
                        Mode = TurnstileTransitionMode.ForwardIn,
                    }
                });

            TransitionService.SetNavigationOutTransition(
                this, new NavigationOutTransition
                {
                    Backward = new TurnstileTransition
                    {
                        Mode = TurnstileTransitionMode.BackwardOut,
                    },
                    Forward = new TurnstileTransition
                    {
                        Mode = TurnstileTransitionMode.ForwardOut,
                    }
                });
        }
    }
}