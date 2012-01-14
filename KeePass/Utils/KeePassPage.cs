using System;
using System.Windows.Navigation;
using KeePass.I18n;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace KeePass.Utils
{
    public class KeePassPage : PhoneApplicationPage
    {
        private ProgressIndicator _indicator;

        public KeePassPage()
        {
            SupportedOrientations = SupportedPageOrientation
                .PortraitOrLandscape;
        }

        protected ProgressIndicator AddIndicator()
        {
            if (_indicator == null)
            {
                _indicator = new ProgressIndicator
                {
                    IsVisible = false,
                    IsIndeterminate = true,
                    Text = Strings.Loading,
                };

                SystemTray.SetProgressIndicator(this, _indicator);
            }

            return _indicator;
        }

        protected ApplicationBarIconButton AppButton(int index)
        {
            return ExtensionMethods.AppButton(this, index);
        }

        protected ApplicationBarMenuItem AppMenu(int index)
        {
            return ExtensionMethods.AppMenu(this, index);
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