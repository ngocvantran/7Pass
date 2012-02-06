using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Navigation;
using KeePass.Data;
using KeePass.I18n;
using KeePass.IO.Data;
using KeePass.Storage;
using KeePass.Utils;

namespace KeePass.Sources.SkyDrive
{
    public partial class List
    {
        private SkyDriveClient _client;
        private string _current;
        private string _folder;

        public List()
        {
            InitializeComponent();
            AppButton(0).Text = Strings.Refresh;
        }

        protected override void OnNavigatedTo(
            bool cancelled, NavigationEventArgs e)
        {
            if (cancelled)
                return;

            var pars = NavigationContext
                .QueryString;

            _folder = pars["folder"];
            _client = new SkyDriveClient(pars["token"]);

            RefreshList(null);
        }

        private static bool IsSupportedFormat(string name)
        {
            return name.EndsWith(".doc",
                StringComparison.InvariantCultureIgnoreCase);
        }

        private void OnFileDownloaded(MetaListItemInfo item,
            string path, byte[] bytes)
        {
            var dispatcher = Dispatcher;

            try
            {
                using (var buffer = new MemoryStream(bytes))
                {
                    if (string.IsNullOrEmpty(_folder))
                    {
                        if (!DatabaseVerifier.Verify(dispatcher, buffer))
                            return;

                        var name = item.Title;
                        if (IsSupportedFormat(name))
                        {
                            name = Path.ChangeExtension(
                                name, null);
                        }
                        name = name.RemoveKdbx();

                        var storage = new DatabaseInfo();
                        storage.SetDatabase(buffer, new DatabaseDetails
                        {
                            Url = path,
                            Name = name,
                            Modified = item.Modified,
                            Type = SourceTypes.Synchronizable,
                            Source = DatabaseUpdater.SKYDRIVE_UPDATER,
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

        private void RefreshList(string path)
        {
            progBusy.IsBusy = true;

            _client.List(path, (parent, items) =>
            {
                try
                {
                    _current = path;

                    var grandParent = parent != null
                        ? parent.Parent : null;

                    if (!string.IsNullOrEmpty(grandParent))
                    {
                        var list = new List<ListItemInfo>(items);
                        list.Insert(0, new ParentItem(grandParent));

                        lstItems.SetItems(list);
                    }
                    else
                        lstItems.SetItems(items);
                }
                finally
                {
                    Dispatcher.BeginInvoke(() =>
                        progBusy.IsBusy = false);
                }
            });
        }

        private void cmdRefresh_Click(object sender, EventArgs e)
        {
            RefreshList(_current);
        }

        private void lstItems_SelectionChanged(object sender,
            NavigationListControl.NavigationEventArgs e)
        {
            if (!Network.CheckNetwork())
                return;

            var item = e.Item as MetaListItemInfo;

            if (item != null)
            {
                if (item.IsDir)
                {
                    RefreshList(item.Path);
                    return;
                }

                if (!IsSupportedFormat(item.Title))
                {
                    var result = MessageBox.Show(
                        Strings.SkyDrive_Unsupported,
                        "SkyDrive", MessageBoxButton.OKCancel);

                    if (result != MessageBoxResult.OK)
                        return;

                    progBusy.IsBusy = true;
                    var name = item.Title + ".doc";

                    _client.Rename(item.Path, name,
                        path => _client.Download(path,
                            OnFileDownloaded));

                    return;
                }

                progBusy.IsBusy = true;
                _client.Download(item.Path,
                    OnFileDownloaded);

                return;
            }

            var parent = e.Item as ParentItem;
            if (parent != null)
                RefreshList(parent.Path);
        }
    }
}