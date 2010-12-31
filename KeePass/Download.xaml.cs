using System;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using KeePass.IO;
using KeePass.Properties;
using KeePass.Services;

namespace KeePass
{
    public partial class Download
    {
        public Download()
        {
            InitializeComponent();

            if (TrialManager.IsTrial)
            {
                txtTrial.Text = string.Concat(AppResources.DemoDb,
                    Environment.NewLine, AppResources.DemoDbTrial);
            }
            else
                txtTrial.Text = AppResources.DemoDb;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (txtUrl.Text != "http://")
                return;

            var url = AppSettingsService.DownloadUrl;
            if (string.IsNullOrEmpty(url))
                return;

            txtUrl.Text = url;
        }

        private static void CopyStream(Stream input, Stream output)
        {
            var buffer = new byte[32768];

            while (true)
            {
                var read = input.Read(buffer, 0, buffer.Length);

                if (read <= 0)
                    return;

                output.Write(buffer, 0, read);
            }
        }

        private void DownloadDatabase()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                MessageBox.Show(AppResources.NoNetwork);
                return;
            }

            UpdateControls(true);

            var client = new WebClient();
            client.OpenReadCompleted += client_OpenReadCompleted;
            client.DownloadProgressChanged += client_DownloadProgressChanged;
            client.OpenReadAsync(new Uri(txtUrl.Text, UriKind.Absolute));

            progress.Value = 0;
            progress.IsIndeterminate = true;
        }

        private static bool IncreaseStorage(long quotaSizeDemand)
        {
            using (var store = IsolatedStorageFile
                .GetUserStoreForApplication())
            {
                var maxAvailableSpace = store.AvailableFreeSpace;

                if (quotaSizeDemand <= maxAvailableSpace)
                    return true;

                return store.IncreaseQuotaTo(
                    store.Quota + quotaSizeDemand);
            }
        }

        private void SaveDb(Stream database)
        {
            using (var store = IsolatedStorageFile
                .GetUserStoreForApplication())
            using (var fs = store.CreateFile(Consts.DATABASE))
            {
                CopyStream(database, fs);
            }

            TrialManager.UpdatedDb();
            AppSettingsService.Clear();
            AppSettingsService.DownloadUrl = txtUrl.Text;

            NavigationService.GoBack();
        }

        private void UpdateControls(bool isDownloading)
        {
            var visible = isDownloading
                ? Visibility.Visible
                : Visibility.Collapsed;

            lblLoad.Visibility = visible;
            progress.Visibility = visible;
            progress.IsIndeterminate = false;

            txtUrl.IsReadOnly = isDownloading;
            cmdDownload.IsEnabled = !isDownloading;
        }

        private void PhoneApplicationPage_BackKeyPress(
            object sender, CancelEventArgs e)
        {
            if (AppSettingsService.HasDatabase())
                return;

            e.Cancel = true;
            App.Quit();
        }

        private void client_DownloadProgressChanged(
            object sender, DownloadProgressChangedEventArgs e)
        {
            progress.IsIndeterminate = false;
            progress.Value = e.ProgressPercentage;

            lblLoad.Text = string.Format(
                AppResources.Downloading,
                e.ProgressPercentage);
        }

        private void client_OpenReadCompleted(
            object sender, OpenReadCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                UpdateControls(false);
                
                var message = AppResources.DownloadError +
                    e.Error.Message;

                MessageBox.Show(message,
                    AppResources.DownloadTitle,
                    MessageBoxButton.OK);

                return;
            }

            var result = e.Result;
            progress.IsIndeterminate = true;
            lblLoad.Text = AppResources.Saving;

            if (result == null || result.Length == 0)
            {
                UpdateControls(false);
                MessageBox.Show(AppResources.NullDownload,
                    AppResources.DownloadTitle,
                    MessageBoxButton.OK);

                return;
            }

            if (!DatabaseReader.CheckSignature(result))
            {
                UpdateControls(false);
                MessageBox.Show(AppResources.NotKdbx,
                    AppResources.DownloadTitle,
                    MessageBoxButton.OK);

                return;
            }

            if (!IncreaseStorage(result.Length))
            {
                UpdateControls(false);
                MessageBox.Show(AppResources.FileTooLarge,
                    AppResources.DownloadTitle,
                    MessageBoxButton.OK);

                return;
            }

            TrialManager.UseRealDb();

            result.Position = 0;
            SaveDb(result);
        }

        private void cmdDownload_Click(object sender, RoutedEventArgs e)
        {
            DownloadDatabase();
        }

        private void lnkDemo_Click(object sender, RoutedEventArgs e)
        {
            if (AppSettingsService.HasDatabase())
            {
                var result = MessageBox.Show(
                    AppResources.ConfirmDemoDb,
                    lnkDemo.Content.ToString(),
                    MessageBoxButton.OKCancel);

                if (result != MessageBoxResult.OK)
                    return;
            }

            TrialManager.UseDemoDb();

            var demoDb = Application.GetResourceStream(
                new Uri("Demo7Pass.kdbx", UriKind.Relative));
            SaveDb(demoDb.Stream);
        }

        private void txtUrl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && cmdDownload.IsEnabled)
                DownloadDatabase();
        }

        private void txtUrl_TextChanged(object sender,
            TextChangedEventArgs e)
        {
            cmdDownload.IsEnabled = Uri.IsWellFormedUriString(
                txtUrl.Text, UriKind.Absolute);
        }
    }
}