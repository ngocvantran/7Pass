using System;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using KeePass.Data;
using KeePass.IO.Data;
using KeePass.Storage;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace KeePass
{
    public partial class WebView
    {
        private Entry _entry;

        public WebView()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(
            bool cancelled, NavigationEventArgs e)
        {
            if (cancelled || _entry != null)
                return;

            var database = Cache.Database;
            if (database == null)
            {
                GoBack<MainPage>();
                return;
            }

            var id = NavigationContext
                .QueryString["entry"];

            _entry = database.GetEntry(id) ??
                CurrentEntry.Entry;

            foreach (var field in _entry.CustomFields.Take(3))
            {
                var item = new ApplicationBarMenuItem(field.Key);
                item.Click += (s, _) => SetValue(field.Value);
                ApplicationBar.MenuItems.Add(item);
            }
        }

        private void SetValue(string value)
        {
            try
            {
                browser.Focus();

                value = value.Replace(@"\", @"\\");
                var script = string.Format(
                    "document.activeElement.value='{0}'", value);
                browser.InvokeScript("eval", script);
            }
            catch (Exception)
            {
                lblOverlay.Text = value;
                vwOverlay.Visibility = Visibility.Visible;
            }
        }

        private void browser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            progBusy.IsIndeterminate = false;
            progBusy.Visibility = Visibility.Collapsed;
        }

        private void browser_Loaded(object sender, RoutedEventArgs e)
        {
            var url = new Uri(
                NavigationContext.QueryString["url"],
                UriKind.Absolute);

            browser.Navigate(url);
        }

        private void browser_Navigated(object sender, NavigationEventArgs e)
        {
            progBusy.IsIndeterminate = true;
            progBusy.Visibility = Visibility.Visible;
        }

        private void cmdBack_Click(object sender, EventArgs e)
        {
            try
            {
                browser.InvokeScript("eval", "history.go(-1)");
            }
            catch {}
        }

        private void cmdPassword_Click(object sender, EventArgs e)
        {
            SetValue(_entry.Password);
        }

        private void cmdRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                browser.InvokeScript("eval", "history.go()");
            }
            catch
            {
                browser.Navigate(browser.Source);
            }
        }

        private void cmdUser_Click(object sender, EventArgs e)
        {
            SetValue(_entry.UserName);
        }

        private void vwOverlay_Flick(object sender, FlickGestureEventArgs e)
        {
            if (e.Direction != System.Windows.Controls.Orientation.Vertical)
                return;

            if (e.VerticalVelocity >= 0)
                return;

            e.Handled = true;
            vwOverlay.Visibility = Visibility.Collapsed;
        }
    }
}