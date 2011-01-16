using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using KeePass.Sources.DropBox.Api;
using KeePass.Storage;
using KeePass.Utils;
using Microsoft.Phone.Shell;

namespace KeePass.Sources.DropBox
{
    public partial class List
    {
        private readonly ApplicationBarIconButton _cmdRefresh;
        private readonly ObservableCollection<MetaListItemInfo> _items;
        private string _path;
        private string _secret;
        private string _token;

        public List()
        {
            InitializeComponent();

            _items = new ObservableCollection<MetaListItemInfo>();
            lstBrowse.ItemsSource = _items;

            _cmdRefresh = (ApplicationBarIconButton)
                ApplicationBar.Buttons[0];
        }

        protected override void OnNavigatedTo(
            bool cancelled, NavigationEventArgs e)
        {
            if (cancelled)
                return;

            InitPars();

            if (_path != "/")
                PageTitle.Text = Path.GetFileName(_path);

            if (Network.CheckNetwork() && _items.Count == 0)
                RefreshList();
        }

        private void InitPars()
        {
            var pars = NavigationContext.QueryString;

            _path = pars["path"];
            _token = pars["token"];
            _secret = pars["secret"];
        }

        private void NavigateTo(string path)
        {
            this.NavigateTo<List>(
                "token={0}&secret={1}&path={2}",
                _token, _secret, path);
        }

        private void OnFileDownloaded(Stream file,
            string path, string title)
        {
            var dispatcher = Dispatcher;

            try
            {
                if (file == null)
                {
                    dispatcher.BeginInvoke(() => MessageBox.Show(
                        DropBoxResources.DownloadError,
                        DropBoxResources.ListTitle,
                        MessageBoxButton.OK));

                    return;
                }

                if (!DatabaseVerifier.Verify(dispatcher, file))
                    return;

                var storage = new DatabaseInfo();
                storage.SetDatabase(file, new DatabaseDetails
                {
                    Url = path,
                    Name = title,
                    Source = DatabaseUpdater.DROPBOX_UPDATER,
                });

                dispatcher.BeginInvoke(
                    GoBack<MainPage>);
            }
            finally
            {
                dispatcher.BeginInvoke(() =>
                {
                    progList.IsLoading = false;
                    _cmdRefresh.IsEnabled = true;
                });
            }
        }

        private void OnListComplete(MetaData data)
        {
            var dispatcher = Dispatcher;

            try
            {
                if (data == null)
                {
                    dispatcher.BeginInvoke(() => MessageBox.Show(
                        DropBoxResources.ListError,
                        DropBoxResources.ListTitle,
                        MessageBoxButton.OK));

                    return;
                }

                dispatcher.BeginInvoke(
                    () => _items.Clear());

                foreach (var child in data.Contents
                    .OrderBy(x => !x.IsDir)
                    .ThenBy(x => x.Name))
                {
                    var meta = child;
                    dispatcher.BeginInvoke(() => _items
                        .Add(new MetaListItemInfo(meta)));

                    Thread.Sleep(50);
                }
            }
            finally
            {
                dispatcher.BeginInvoke(() =>
                {
                    progList.IsLoading = false;
                    _cmdRefresh.IsEnabled = true;
                });
            }
        }

        private void RefreshList()
        {
            progList.IsLoading = true;
            _cmdRefresh.IsEnabled = false;

            new Client(_token, _secret).ListAsync(
                _path, OnListComplete);
        }

        private void cmdRefresh_Click(object sender, EventArgs e)
        {
            RefreshList();
        }

        private void lstBrowse_SelectionChanged(
            object sender, SelectionChangedEventArgs e)
        {
            var meta = lstBrowse.SelectedItem as MetaListItemInfo;

            if (meta == null)
                return;

            if (Network.CheckNetwork())
            {
                if (meta.IsDir)
                    NavigateTo(meta.Path);
                else
                {
                    progList.IsLoading = true;
                    _cmdRefresh.IsEnabled = false;

                    var url = string.Format(
                        "dropbox://{0}:{1}@dropbox.com{2}",
                        _token, _secret, meta.Path);

                    var client = new Client(_token, _secret);
                    client.DownloadAsync(meta.Path, x =>
                        OnFileDownloaded(x, url, meta.Title));
                }
            }

            lstBrowse.SelectedItem = null;
        }
    }
}