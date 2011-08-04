using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Threading;
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
        private WebDavClient _client;

        private string _folder;
        private bool _loaded;
        private string _path;

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

            _client = new WebDavClient(
                pars["user"], pars["pass"]);

            _path = pars["path"];
            _folder = pars["folder"];
        }

        private void NavigateTo(string path)
        {
            this.NavigateTo<WebList>(
                "user={0}&pass={1}&path={2}&folder={3}",
                _client.User, _client.Password, path, _folder);
        }

        private void OnFileDownloadFailed(WebException obj)
        {
            Dispatcher.BeginInvoke(() =>
            {
                progBusy.IsBusy = false;

                MessageBox.Show(
                    WebDavResources.DownloadError,
                    WebDavResources.ListTitle,
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
                            Name = title,
                            Modified = modified,
                            Url = _client.GetUrl(path),
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
                    this.BackToDBs);
            }
            finally
            {
                dispatcher.BeginInvoke(() =>
                    progBusy.IsBusy = false);
            }
        }

        private void OnHtmlDetected()
        {
            Dispatcher.BeginInvoke(() =>
            {
                var result = MessageBox.Show(
                    WebDavResources.HtmlDetected,
                    WebDavResources.HtmlDetectedTitle,
                    MessageBoxButton.OKCancel);

                if (result != MessageBoxResult.OK)
                {
                    OnListFailed();
                    return;
                }

                this.NavigateTo<Web.WebDownload>(
                    "folder={0}&url={1}&user={2}&pass={3}",
                    _folder, _path,
                    _client.User, _client.Password);
            });
        }

        private void OnListComplete(IList<ItemInfo> itemInfos)
        {
            try
            {
                var items = itemInfos
                    .Select(x => new MetaListItemInfo(_path, x))
                    .Where(x => x.Path != _path)
                    .OrderBy(x => !x.IsDir)
                    .ThenBy(x => x.Title)
                    .ToList();

                lstBrowse.SetItems(items);
            }
            finally
            {
                Dispatcher.BeginInvoke(() =>
                {
                    progBusy.IsBusy = false;
                    _cmdRefresh.IsEnabled = true;
                });
            }
        }

        private void OnListFailed(WebException webException)
        {
            OnListFailed();
        }

        private void OnListFailed()
        {
            Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show(
                    WebDavResources.ListError,
                    WebDavResources.ListTitle,
                    MessageBoxButton.OK);

                NavigationService.GoBack();
            });
        }

        private void RefreshList()
        {
            progBusy.IsBusy = true;
            _cmdRefresh.IsEnabled = false;

            _client.ListAsync(_path,
                OnListComplete,
                OnHtmlDetected,
                OnListFailed);
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

                _client.DownloadAsync(meta.Path, x =>
                    OnFileDownloaded(x, meta.Path,
                        meta.Title, meta.Modified),
                    OnFileDownloadFailed);
            }
        }
    }
}