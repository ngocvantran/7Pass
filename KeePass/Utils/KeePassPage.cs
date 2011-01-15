using System;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;

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
            NavigationService.GoBack();
        }

        public void GoBack<T>()
            where T : PhoneApplicationPage
        {
            GoBack(Navigation.GetPathTo<T>());
        }

        [Obsolete("Please override the other override", true)]
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

            OnNavigatedTo(_backTarget != null, e);
        }

        protected virtual void OnNavigatedTo(
            bool cancelled, NavigationEventArgs e) {}
    }
}