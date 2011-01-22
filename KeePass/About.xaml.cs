using System;
using System.Reflection;
using System.Windows;
using Microsoft.Phone.Tasks;

namespace KeePass
{
    public partial class About
    {
        public About()
        {
            InitializeComponent();

            var asm = Assembly.GetExecutingAssembly();
            var parts = asm.FullName.Split(',');
            var version = parts[1].Split('=')[1];

            lblVersion.Text = string.Format(
                lblVersion.Text, version);
        }

        private void lnkMartket_Click(
            object sender, RoutedEventArgs e)
        {
            new MarketplaceDetailTask().Show();
        }
    }
}