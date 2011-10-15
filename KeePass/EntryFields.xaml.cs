using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using KeePass.Controls;
using KeePass.Data;
using KeePass.Storage;
using KeePass.Utils;

namespace KeePass
{
    public partial class EntryFields
    {
        private EntryBinding _entry;
        private ObservableCollection<FieldBinding> _fields;

        public EntryFields()
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

            _entry = CurrentEntry.Entry;
            _fields = new ObservableCollection
                <FieldBinding>(_entry.GetFields()
                    .Select(x => new FieldBinding(x)));

            lstFields.ItemsSource = _fields;
        }

        private void cmdAbout_Click(object sender, EventArgs e)
        {
            this.NavigateTo<About>();
        }

        private void cmdAdd_Click(object sender, EventArgs e)
        {
            _entry.HasChanges = true;
            
            var field = _entry.AddField();
            _fields.Add(new FieldBinding(field));
        }

        private void cmdHome_Click(object sender, EventArgs e)
        {
            this.BackToRoot();
        }

        private void cmdRoot_Click(object sender, EventArgs e)
        {
            this.BackToDBs();
        }

        private void txtField_GotFocus(
            object sender, RoutedEventArgs e)
        {
            var txtField = (ProtectedTextBox)sender;
            txtField.SelectAll();
        }

        private void txtField_TextChanged(
            object sender, TextChangedEventArgs e)
        {
            var txtField = (ProtectedTextBox)sender;
            var field = (FieldBinding)txtField.Tag;

            if (field.Value == txtField.Text)
                return;

            _entry.HasChanges = true;
            field.Value = txtField.Text;
        }

        private void txtName_GotFocus(
            object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        private void txtName_TextChanged(
            object sender, TextChangedEventArgs e)
        {
            var txtName = (TextBox)sender;
            var field = (FieldBinding)txtName.Tag;

            if (field.Name == txtName.Text)
                return;

            _entry.HasChanges = true;
            field.Name = txtName.Text;
        }
    }
}