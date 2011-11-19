using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Navigation;
using KeePass.Data;

namespace KeePass.Sources.SkyDrive
{
    public partial class List
    {
        private SkyDriveClient _client;
        private string _current;

        public List()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(
            bool cancelled, NavigationEventArgs e)
        {
            if (cancelled)
                return;

            var token = NavigationContext
                .QueryString["token"];

            _client = new SkyDriveClient(token);

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
                RefreshList(item.Path);
                return;
            }

            var parent = e.Item as ParentItem;
            if (parent != null)
                RefreshList(parent.Path);
        }
    }
}