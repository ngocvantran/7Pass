using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace KeePass.Utils
{
    public class KeePassPage : PhoneApplicationPage
    {
        private static Uri _backTarget;

        public void GoBack(Uri uri)
        {
            if (!NavigationService.CanGoBack)
                return;

            _backTarget = uri;
            App.Current.RootFrame.Visibility =
                Visibility.Collapsed;

            NavigationService.GoBack();
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (_backTarget != null && e.Uri == _backTarget)
                _backTarget = null;

            if (_backTarget != null)
            {
                if (NavigationService.CanGoBack)
                    NavigationService.GoBack();
                else
                    _backTarget = null;
            }

            var cancelled = _backTarget != null;

            if (!cancelled)
            {
                App.Current.RootFrame.Visibility =
                    Visibility.Visible;

                var globalPass = AppSettings
                    .Instance.GlobalPass;

                if (globalPass.ShouldPromptGlobalPass)
                {
                    OnNavigatedTo(true, e);
                    this.NavigateTo<GlobalPassVerify>();

                    return;
                }
            }

            OnNavigatedTo(cancelled, e);
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
                container.Children.Add(new TrialNotification());
        }
    }
}