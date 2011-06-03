using System;
using System.Windows.Controls;
using System.Windows.Navigation;
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
                GoBack<MainPage>();
                return;
            }

            var id = NavigationContext.QueryString["id"];
            DataContext = _entry = database.GetEntry(id);
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

        private void txtNotes_Changed(
            object sender, TextChangedEventArgs e)
        {
            _entry.Notes = txtNotes.Text;
            EntryState.HasChanges = true;
        }
    }
}