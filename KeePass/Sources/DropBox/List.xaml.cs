using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using KeePass.IO;
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
        private readonly ProgressIndicator _progList;
        private string _folder;

        private string _path;
        private string _secret;
        private string _token;

        public List()
        {
            InitializeComponent();

            _progList = GetIndicator();

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
            _folder = pars["folder"];
            _secret = pars["secret"];
        }

        private void NavigateTo(string path)
        {
            this.NavigateTo<List>(
                "token={0}&secret={1}&path={2}&folder={3}",
                _token, _secret, path, _folder);
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

                if (string.IsNullOrEmpty(_folder))
                {
                    if (!DatabaseVerifier.Verify(dispatcher, file))
                        return;

                    var storage = new DatabaseInfo();
                    storage.SetDatabase(file, new DatabaseDetails
                    {
                        Url = path,
                        Name = title,
                        Source = DatabaseUpdater.DROPBOX_UPDATER,
                    });
                }
                else
                {
                    var hash = KeyFile.GetKey(file);
                    if (hash == null)
                    {
                        dispatcher.BeginInvoke(() => MessageBox.Show(
                            Properties.Resources.InvalidKeyFile,
                            Properties.Resources.KeyFileTitle,
                            MessageBoxButton.OK));

                        return;
                    }

                    new DatabaseInfo(_folder)
                        .SetKeyFile(hash);
                }

                dispatcher.BeginInvoke(() =>
                    this.BackToDatabases());
            }
            finally
            {
                dispatcher.BeginInvoke(
                    () => SetWorking(false));
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
                    _progList.IsVisible = false;
                    _cmdRefresh.IsEnabled = true;
                });
            }
        }

        private void RefreshList()
        {
            _progList.IsVisible = true;
            _cmdRefresh.IsEnabled = false;

            new Client(_token, _secret).ListAsync(
                _path, OnListComplete);
        }

        private void SetWorking(bool working)
        {
            _progList.IsVisible = working;
            lstBrowse.IsEnabled = !working;
            _cmdRefresh.IsEnabled = !working;
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
                    var url = string.Format(
                        "dropbox://{0}:{1}@dropbox.com{2}",
                        _token, _secret, meta.Path);

                    SetWorking(true);

                    var client = new Client(_token, _secret);
                    client.DownloadAsync(meta.Path, x =>
                        OnFileDownloaded(x, url, meta.Title));
                }
            }

            lstBrowse.SelectedItem = null;
        }
    }
}