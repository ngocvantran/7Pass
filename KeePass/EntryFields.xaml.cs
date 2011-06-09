using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using KeePass.Data;
using KeePass.IO.Data;
using KeePass.Storage;
using KeePass.Utils;

namespace KeePass
{
    public partial class EntryFields
    {
        private Entry _entry;

        public EntryFields()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(
            bool cancelled, NavigationEventArgs e)
        {
            if (cancelled)
                return;

            var database = Cache.Database;
            if (database == null)
            {
                GoBack<MainPage>();
                return;
            }

            var id = NavigationContext
                .QueryString["id"];

            _entry = database.GetEntry(id)
                ?? CurrentEntry.Entry;

            DataContext = _entry;
        }

        private void cmdAbout_Click(object sender, EventArgs e)
        {
            this.NavigateTo<About>();
        }

        private void cmdHome_Click(object sender, EventArgs e)
        {
            GoBack<GroupDetails>();
        }

        private void cmdRoot_Click(object sender, EventArgs e)
        {
            GoBack<MainPage>();
        }

        private void txtField_GotFocus(
            object sender, RoutedEventArgs e)
        {
            var txtField = (TextBox)sender;
            txtField.SelectAll();
        }

        private void txtField_TextChanged(
            object sender, TextChangedEventArgs e)
        {
            var txtField = (TextBox)sender;
            var key = (string)txtField.Tag;

            if (_entry[key] == txtField.Text)
                return;

            _entry[key] = txtField.Text;
            CurrentEntry.HasChanges = true;
        }
    }
}