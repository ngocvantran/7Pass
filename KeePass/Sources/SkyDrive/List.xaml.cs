using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Navigation;
using KeePass.Data;
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
        }

        protected override void OnNavigatedTo(
            bool cancelled, NavigationEventArgs e)
        {
            if (cancelled)
                return;

            var pars = NavigationContext
                .QueryString;

            _client = new SkyDriveClient(
                pars["token"]);
            _folder = pars["folder"];

            RefreshList(null);
            DisplayEmail();
        }

        private void DisplayEmail()
        {
            _client.GetEmail(email =>
            {
                if (!string.IsNullOrEmpty(email))
                {
                    Dispatcher.BeginInvoke(() =>
                        lblEmail.Text = email);
                }
            });
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

                        var storage = new DatabaseInfo();
                        storage.SetDatabase(buffer, new DatabaseDetails
                        {
                            Url = path,
                            Modified = item.Modified,
                            Name = item.Title.RemoveKdbx(),
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
                /*dispatcher.BeginInvoke(() =>
                    progBusy.IsBusy = false);*/
            }
        }

        private void RefreshList(string path)
        {
            _client.List(path, (parent, items) =>
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
            });
        }

        private void lnkRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshList(_current);
        }

        private void lstItems_SelectionChanged(object sender,
            NavigationListControl.NavigationEventArgs e)
        {
            var item = e.Item as MetaListItemInfo;

            if (item != null)
            {
                if (item.IsDir)
                {
                    RefreshList(item.Path);
                    return;
                }

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