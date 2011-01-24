using System;
using System.Windows;
using System.Windows.Navigation;
using KeePass.Storage;
using KeePass.Utils;

namespace KeePass.Sources
{
    public partial class Download
    {
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
        }

        private void lnkDemo_Click(object sender, EventArgs e)
        {
            var info = new DatabaseInfo();
            var demoDb = Application.GetResourceStream(
                new Uri("Sources/Demo7Pass.kdbx", UriKind.Relative));

            info.SetDatabase(demoDb.Stream, new DatabaseDetails
            {
                Source = "Demo",
                Name = "Demo Database",
            });

            MessageBox.Show(
                Properties.Resources.DemoDbText,
                Properties.Resources.DemoDbTitle,
                MessageBoxButton.OK);

            GoBack<MainPage>();
        }
    }
}