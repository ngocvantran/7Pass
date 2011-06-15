using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using DropNet;
using DropNet.Exceptions;
using DropNet.Models;
using KeePass.IO.Data;
using KeePass.Storage;
using KeePass.Utils;
using Microsoft.Phone.Shell;

namespace KeePass.Sources.DropBox
{
    public partial class List
    {
        private readonly ApplicationBarIconButton _cmdRefresh;
        private readonly ObservableCollection<MetaListItemInfo> _items;
        private string _folder;

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
            _folder = pars["folder"];
            _secret = pars["secret"];
        }

        private void NavigateTo(string path)
        {
            this.NavigateTo<List>(
                "token={0}&secret={1}&path={2}&folder={3}",
                _token, _secret, path, _folder);
        }

        private void OnFileDownloadFailed(DropboxException obj)
        {
            progBusy.IsBusy = false;

            Dispatcher.BeginInvoke(() =>
                MessageBox.Show(
                    DropBoxResources.DownloadError,
                    DropBoxResources.ListTitle,
                    MessageBoxButton.OK));
        }

        private void OnFileDownloaded(byte[] file,
            string path, string title, string modified)
        {
            var dispatcher = Dispatcher;

            try
            {
                using (var buffer = new MemoryStream(file))
                {
                    if (string.IsNullOrEmpty(_folder))
                    {
                        if (!DatabaseVerifier.Verify(dispatcher, buffer))
                            return;

                        var storage = new DatabaseInfo();
                        storage.SetDatabase(buffer, new DatabaseDetails
                        {
                            Url = path,
                            Name = title,
                            Modified = modified,
                            Source = DatabaseUpdater.DROPBOX_UPDATER,
                        });
                    }
                    else
                    {
                        var hash = KeyFile.GetKey(buffer);
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
                }

                dispatcher.BeginInvoke(
                    GoBack<MainPage>);
            }
            finally
            {
                dispatcher.BeginInvoke(() =>
                    progBusy.IsBusy = false);
            }
        }

        private void OnListComplete(MetaData data)
        {
            var dispatcher = Dispatcher;

            try
            {
                dispatcher.BeginInvoke(
                    () => _items.Clear());

                foreach (var child in data.Contents
                    .OrderBy(x => !x.Is_Dir)
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
                    progBusy.IsBusy = false;
                    _cmdRefresh.IsEnabled = true;
                });
            }
        }

        private void OnListFailed(DropboxException ex)
        {
            Dispatcher.BeginInvoke(() =>
                MessageBox.Show(
                    DropBoxResources.ListError,
                    DropBoxResources.ListTitle,
                    MessageBoxButton.OK));
        }

        private void RefreshList()
        {
            progBusy.IsBusy = true;
            _cmdRefresh.IsEnabled = false;

            var client = new DropNetClient(
                DropBoxInfo.KEY, DropBoxInfo.SECRET,
                _token, _secret);

            client.GetMetaDataAsync(_path,
                OnListComplete, OnListFailed);
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
                    progBusy.IsBusy = true;

                    var client = DropBoxInfo
                        .Create(_token, _secret);

                    var url = client.GetUrl(meta.Path);
                    client.GetFileAsync(meta.Path,
                        x => OnFileDownloaded(x.RawBytes, url,
                            meta.Title, meta.Modified),
                        OnFileDownloadFailed);
                }
            }

            lstBrowse.SelectedItem = null;
        }
    }
}