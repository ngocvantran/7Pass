using System;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace KeePass.Utils
{
    public class KeePassPage : PhoneApplicationPage
    {
        /// <summary>
        /// Gets the system tray progress indicator.
        /// </summary>
        /// <returns>System tray progress indicator.</returns>
        protected ProgressIndicator GetIndicator()
        {
            var indicator = new ProgressIndicator
            {
                IsVisible = false,
                IsIndeterminate = true,
            };

            SystemTray.SetProgressIndicator(
                this, indicator);

            return indicator;
        }

        protected override void OnNavigatedTo(
            NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var globalPass = AppSettings
                .Instance.GlobalPass;

            if (globalPass.ShouldPromptGlobalPass)
            {
                OnNavigatedTo(true, e);
                this.NavigateTo<GlobalPassVerify>();

                return;
            }

            SetTransition(e);
            OnNavigatedTo(false, e);

            ShowTrialNotification();
        }

        protected virtual void OnNavigatedTo(
            bool cancelled, NavigationEventArgs e) {}

        private void SetTransition(NavigationEventArgs e)
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

        private void ShowTrialNotification()
        {
            if (!TrialManager.ShouldShowPopup())
                return;

            var container = Content as Panel;
            if (container != null)
            {
                container.Children.Add(
                    new TrialNotification());
            }
        }
    }
}