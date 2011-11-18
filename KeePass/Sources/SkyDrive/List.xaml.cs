using System;

namespace KeePass.Sources.SkyDrive
{
    public partial class List
    {
        public List()
        {
            InitializeComponent();
        }

        private void mnuLogout_Click(object sender, EventArgs e)
        {
            LiveAuth.AttemptLogout = true;
            NavigationService.GoBack();
        }
    }
}