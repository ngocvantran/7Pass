using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Navigation;
using KeePass.IO.Data;
using KeePass.Sources.WebDav.Api;
using KeePass.Storage;
using KeePass.Utils;
using Microsoft.Phone.Shell;

namespace KeePass.Sources.WebDav
{
    public partial class WebList
    {
        private readonly ApplicationBarIconButton _cmdRefresh;

        private string _folder;
        private bool _loaded;
        private string _pass;
        private string _path;
        private string _server;
        private string _user;

        public WebList()
        {
            InitializeComponent();

            _cmdRefresh = (ApplicationBarIconButton)
                ApplicationBar.Buttons[0];
        }

        protected override void OnNavigatedTo(
            bool cancelled, NavigationEventArgs e)
        {
            if (cancelled)
                return;

            InitPars();

            if (_path != ".")
                PageTitle.Text = Path.GetFileName(_path);

            if (Network.CheckNetwork() && !_loaded)
                RefreshList();

            _loaded = true;
        }

        private void InitPars()
        {
            var pars = NavigationContext.QueryString;

            _path = pars["path"];
            _user = pars["user"];
            _pass = pars["pass"];
            _server = pars["server"];
            _folder = pars["folder"];
        }

        private void NavigateTo(string path)
        {
            this.NavigateTo<WebList>(
                "server={0}&user={1}&pass={2}&path={3}&folder={4}",
                _server, _user, _pass, path, _folder);
        }

        private void OnFileDownloadFailed(WebException obj)
        {
            progBusy.IsBusy = false;

            Dispatcher.BeginInvoke(() =>
                MessageBox.Show(
                    WebDavResources.DownloadError,
                    WebDavResources.ListTitle,
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
                            Type = SourceTypes.Synchronizable,
                            Source = DatabaseUpdater.WEBDAV_UPDATER,
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

        private void OnListComplete(IList<ItemInfo> itemInfos)
        {
            var dispatcher = Dispatcher;

            try
            {
                var items = itemInfos
                    .Where(x => x.Path != _path)
                    .Select(x => new MetaListItemInfo(x))
                    .OrderBy(x => !x.IsDir)
                    .ThenBy(x => x.Title)
                    .ToList();

                lstBrowse.SetItems(items);
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

        private void OnListFailed(WebException webException)
        {
            Dispatcher.BeginInvoke(() =>
                MessageBox.Show(
                    WebDavResources.ListError,
                    WebDavResources.ListTitle,
                    MessageBoxButton.OK));
        }

        private void RefreshList()
        {
            progBusy.IsBusy = true;
            _cmdRefresh.IsEnabled = false;

            var client = new WebDavClient(
                _server, _user, _pass);
            client.ListAsync(_path,
                OnListComplete, OnListFailed);
        }

        private void cmdRefresh_Click(object sender, EventArgs e)
        {
            RefreshList();
        }

        private void lstBrowse_SelectionChanged(object sender,
            NavigationListControl.NavigationEventArgs e)
        {
            var meta = e.Item as MetaListItemInfo;

            if (meta == null)
                return;

            if (!Network.CheckNetwork())
                return;

            if (meta.IsDir)
                NavigateTo(meta.Path);
            else
            {
                progBusy.IsBusy = true;

                var client = new WebDavClient(
                    _server, _user, _pass);
                client.DownloadAsync(meta.Path, x =>
                    OnFileDownloaded(x, meta.Path,
                        meta.Title, meta.Modified),
                    OnFileDownloadFailed);
            }
        }
    }
}