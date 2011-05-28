using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace KeePass.Utils
{
    public class KeePassPage : PhoneApplicationPage
    {
        public void GoBack(Uri uri)
        {
            var service = NavigationService;
            if (!service.CanGoBack)
                return;

            var backStack = service.BackStack
                .Select(x => x.Source)
                .ToList();

            var index = backStack.IndexOf(uri);
            if (index < 0)
                return;

            for (var i = 0; i < index; i++)
                service.RemoveBackEntry();

            service.GoBack();
        }

        public void GoBack<T>()
            where T : PhoneApplicationPage
        {
            GoBack(Navigation.GetPathTo<T>());
        }

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

            OnNavigatedTo(false, e);
            ShowTrialNotification();
        }

        protected virtual void OnNavigatedTo(
            bool cancelled, NavigationEventArgs e) {}

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