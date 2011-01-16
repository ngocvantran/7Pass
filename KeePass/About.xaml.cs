using System;
using System.Windows;
using Microsoft.Phone.Tasks;

namespace KeePass
{
    public partial class About
    {
        public About()
        {
            InitializeComponent();
        }

        private void lnkMartket_Click(object sender, RoutedEventArgs e)
        {
            new MarketplaceDetailTask().Show();
        }
    }
}