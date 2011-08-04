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
    public partial class EntryNotes
    {
        private Entry _entry;

        public EntryNotes()
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
                this.BackToDBs();
                return;
            }

            var id = NavigationContext
                .QueryString["id"];

            _entry = database.GetEntry(id)
                ?? CurrentEntry.Entry;
            
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

            _entry.Notes = txtNotes.Text;
            CurrentEntry.HasChanges = true;
        }

        private void txtNotes_Loaded(
            object sender, RoutedEventArgs e)
        {
            txtNotes.Focus();
        }
    }
}