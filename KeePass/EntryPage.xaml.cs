using System;
using System.Windows.Navigation;
using KeePass.Data;
using KeePass.IO;

namespace KeePass
{
    public partial class EntryPage
    {
        public EntryPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (LifeCycle.CheckDbState(this) ==
                DatabaseCheckResults.Terminate)
            {
                return;
            }

            DataContext = GetEntry();
        }

        private Entry GetEntry()
        {
            var db = KeyCache.Database;
            var entryId = NavigationContext.QueryString["id"];

            return db.GetEntry(new Guid(entryId));
        }

        private void ApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            this.OpenSettings();
        }
    }
}