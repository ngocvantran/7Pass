using System;
using KeePass.Sources;
using KeePass.Utils;

namespace KeePass
{
    public partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void mnuNew_Click(object sender, EventArgs e)
        {
            this.NavigateTo<Download>();
        }
    }
}