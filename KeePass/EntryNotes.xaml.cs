using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using KeePass.Data;
using KeePass.I18n;
using KeePass.Storage;
using KeePass.Utils;

namespace KeePass
{
    public partial class EntryNotes
    {
        private EntryBinding _entry;

        public EntryNotes()
        {
            InitializeComponent();

            AppMenu(0).Text = Strings.EntryNotes_ClearAll;
            AppButton(0).Text = Strings.App_Home;
            AppButton(1).Text = Strings.App_Databases;
            AppButton(2).Text = Strings.App_About;
        }

        protected override void OnNavigatedTo(
            bool cancelled, NavigationEventArgs e)
        {
            if (cancelled)
                return;

            var database = Cache.Database;
            if (database == null)
            {
                this.BackToDBs();
                return;
            }

            _entry = CurrentEntry.Entry;

            txtNotes.Text = _entry.Notes
                ?? string.Empty;
        }

        private void cmdAbout_Click(object sender, EventArgs e)
        {
            this.NavigateTo<About>();
        }

        private void cmdHome_Click(object sender, EventArgs e)
        {
            this.BackToRoot();
        }

        private void cmdRoot_Click(object sender, EventArgs e)
        {
            this.BackToDBs();
        }

        private void mnuClear_Click(object sender, EventArgs e)
        {
            txtNotes.Text = string.Empty;
        }

        private void txtNotes_Changed(
            object sender, TextChangedEventArgs e)
        {
            if (txtNotes.Text == _entry.Notes)
                return;

            _entry.HasChanges = true;
            _entry.Notes = txtNotes.Text;
        }

        private void txtNotes_Loaded(
            object sender, RoutedEventArgs e)
        {
            txtNotes.Focus();
        }
    }
}
