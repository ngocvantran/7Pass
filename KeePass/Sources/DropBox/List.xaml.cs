using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using DropNet.Exceptions;
using DropNet.Models;
using KeePass.I18n;
using KeePass.IO.Data;
using KeePass.Storage;
using KeePass.Utils;
using Microsoft.Phone.Shell;

namespace KeePass.Sources.DropBox
{
    public partial class List
    {
        private readonly ApplicationBarIconButton _cmdRefresh;

        private string _folder;
        private bool _loaded;
        private string _path;
        private string _secret;
        private string _token;

        public List()
        {
            InitializeComponent();

            _cmdRefresh = AppButton(0);
            _cmdRefresh.Text = Strings.Refresh;
        }

        protected override void OnNavigatedTo(
            bool cancelled, NavigationEventArgs e)
        {
            if (cancelled)
                return;

            InitPars();

            if (_path != "/")
                PageTitle.Text = Path.GetFileName(_path);

            if (Network.CheckNetwork() && !_loaded)
                RefreshList();

            _loaded = true;
        }

        private void InitPars()
        {
            var pars = NavigationContext.QueryString;

            _path = "/";
            _token = pars["token"];
            _folder = pars["folder"];
            _secret = pars["secret"];
        }

        private void NavigateTo(string path)
        {
            _path = path;
            RefreshList();
        }

        private void OnFileDownloadFailed(DropboxException obj)
        {
            Dispatcher.BeginInvoke(() =>
            {
                progBusy.IsBusy = false;

                MessageBox.Show(
                    DropBoxResources.DownloadError,
                    DropBoxResources.ListTitle,
                    MessageBoxButton.OK);
            });
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
                            Modified = modified,
                            Name = title.RemoveKdbx(),
                            Type = SourceTypes.Synchronizable,
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
                    this.BackToDBs);
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
                var items = data.Contents
                    .OrderBy(x => !x.Is_Dir)
                    .ThenBy(x => x.Name)
                    .Select(x => new MetaListItemInfo(x))
                    .ToList();

                if (data.Path != "/")
                {
                    var sepIndex = data.Path.LastIndexOf(
                        "/", StringComparison.Ordinal);

                    if (sepIndex == 0)
                        sepIndex = 1;

                    var grandParent = data.Path.Remove(sepIndex);
                    items.Insert(0, new MetaListItemInfo(grandParent));
                }

                lstItems.SetItems(items);
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

            var client = DropBoxUtils.Create(
                _token, _secret);

            client.GetMetaDataAsync(_path,
                OnListComplete, OnListFailed);
        }

        private void cmdRefresh_Click(object sender, EventArgs e)
        {
            RefreshList();
        }

        private void lstItems_SelectionChanged(object sender,
            NavigationListControl.NavigationEventArgs e)
        {
            var meta = e.Item as MetaListItemInfo;

            if (meta == null)
                return;

            if (!Network.CheckNetwork())
                return;

            if (meta.IsDir)
            {
                NavigateTo(meta.Path);
                return;
            }

            progBusy.IsBusy = true;

            var client = DropBoxUtils
                .Create(_token, _secret);

            var url = client.GetUrl(meta.Path);
            client.GetFileAsync(meta.Path,
                x => OnFileDownloaded(x.RawBytes, url,
                    meta.Title, meta.Modified),
                OnFileDownloadFailed);
        }
    }
}