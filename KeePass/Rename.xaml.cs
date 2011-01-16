using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using KeePass.Storage;
using KeePass.Utils;

namespace KeePass
{
    public partial class Rename
    {
        private DatabaseInfo _database;
        private string _originalName;

        public Rename()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(
            bool cancelled, NavigationEventArgs e)
        {
            if (cancelled)
                return;

            _database = new DatabaseInfo(
                NavigationContext.QueryString["db"]);
            _database.LoadDetails();

            _originalName = _database.Details.Name;

            lblRename.Text = string.Format(
                lblRename.Text, _originalName);
        }

        private void PerformClear()
        {
            txtName.Text = _originalName;
            txtName.SelectAll();
            txtName.Focus();
        }

        private void PerformRename()
        {
            _database.Details.Name = txtName.Text;
            _database.SaveDetails();

            NavigationService.GoBack();
        }

        private void cmdClear_Click(object sender, EventArgs e)
        {
            PerformClear();
        }

        private void cmdRename_Click(object sender, EventArgs e)
        {
            PerformRename();
        }

        private void txtName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.IsEnter() && cmdRename.IsEnabled)
                PerformRename();
        }

        private void txtName_Loaded(object sender, RoutedEventArgs e)
        {
            PerformClear();
        }

        private void txtName_TextChanged(
            object sender, TextChangedEventArgs e)
        {
            var name = txtName.Text;
            cmdRename.IsEnabled = !string.IsNullOrEmpty(name) &&
                name != _originalName;
        }
    }
}