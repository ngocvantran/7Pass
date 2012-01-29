using System;
using System.Windows;
using Coding4Fun.Phone.Controls.Data;
using Microsoft.Phone.Tasks;

namespace KeePass
{
    public partial class About
    {
        public About()
        {
            InitializeComponent();

            var version = PhoneHelper
                .GetAppAttribute("Version");

            lblVersion.Text = string.Format(
                lblVersion.Text, version);
        }

        private void lnkPurchase_Click(
            object sender, RoutedEventArgs e)
        {
            new MarketplaceDetailTask().Show();
        }

        private void lnkReview_Click(
            object sender, RoutedEventArgs e)
        {
            new MarketplaceReviewTask().Show();
        }
    }
}