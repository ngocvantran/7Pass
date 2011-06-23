using System;
using System.Windows;
using System.Windows.Navigation;
using KeePass.Analytics;
using KeePass.Storage;
using KeePass.Utils;
using Microsoft.Phone.Controls;

namespace KeePass.Sources
{
    public partial class Download
    {
        private string _folder;

        public Download()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(
            bool cancelled, NavigationEventArgs e)
        {
            if (cancelled)
                return;

            var isTrial = TrialManager.IsTrial;
            lnkDemo.Visibility = isTrial
                ? Visibility.Visible
                : Visibility.Collapsed;

            ApplicationBar.IsVisible = !isTrial;
            _folder = NavigationContext.QueryString["folder"];
        }

        private void Navigate<T>()
            where T : PhoneApplicationPage
        {
            this.NavigateTo<T>("folder={0}", _folder);
        }

        private void lnkDemo_Click(object sender, EventArgs e)
        {
            var info = new DatabaseInfo();
            var demoDb = Application.GetResourceStream(
                new Uri("Sources/Demo7Pass.kdbx", UriKind.Relative));

            AnalyticsTracker.Track("new_db_demo");

            info.SetDatabase(demoDb.Stream, new DatabaseDetails
            {
                Source = "Demo",
                Name = "Demo Database",
                Type = SourceTypes.OneTime,
            });

            MessageBox.Show(
                Properties.Resources.DemoDbText,
                Properties.Resources.DemoDbTitle,
                MessageBoxButton.OK);

            GoBack<MainPage>();
        }

        private void lnkDropBox_Click(object sender, RoutedEventArgs e)
        {
            AnalyticsTracker.Track("new_db_dropbox");
            Navigate<DropBox.DropBox>();
        }

        private void lnkWeb_Click(object sender, RoutedEventArgs e)
        {
            AnalyticsTracker.Track("new_db_web");
            Navigate<Web.WebDownload>();
        }
    }
}