using System;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;

namespace KeePass.Utils
{
    public class KeePassPage : PhoneApplicationPage
    {
        private static Type _backTarget;

        public void GoBack<T>()
            where T : KeePassPage
        {
            _backTarget = typeof(T);
            NavigationService.GoBack();
        }

        [Obsolete("Please override the other override", true)]
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (_backTarget != null)
            {
                if (GetType() == _backTarget)
                    _backTarget = null;
                else
                {
                    OnNavigatedTo(true, e);
                    NavigationService.GoBack();

                    return;
                }
            }

            OnNavigatedTo(false, e);
        }

        protected virtual void OnNavigatedTo(
            bool cancelled, NavigationEventArgs e) {}
    }
}