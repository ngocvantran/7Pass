using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using Coding4Fun.Phone.Controls;
using KeePass.Controls;
using KeePass.Data;
using KeePass.Storage;
using KeePass.Utils;
using Microsoft.Phone.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

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

        private void chkProtect_CheckedChanged(object sender, RoutedEventArgs e)
        {
            _entry.HasChanges = true;
        }

        private void cmdAbout_Click(object sender, EventArgs e)
        {
            this.NavigateTo<About>();
        }

        private void cmdAdd_Click(object sender, EventArgs e)
        {
            _entry.HasChanges = true;

            var field = _entry.AddField();
            var binding = new FieldBinding(field);
            
            _fields.Add(binding);
            lstFields.UpdateLayout();
            lstFields.ScrollIntoView(binding);

            Dispatcher.BeginInvoke(() =>
                binding.IsEditing = true);
        }

        private void cmdHome_Click(object sender, EventArgs e)
        {
            this.BackToRoot();
        }

        private void cmdRename_Tap(object sender, GestureEventArgs e)
        {
            var cmdRename = (RoundButton)sender;
            var field = (FieldBinding)cmdRename.Tag;

            field.IsEditing = true;
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

        private void txtName_ActionIconTapped(object sender, EventArgs e)
        {
            var element = (FrameworkElement)sender;
            while (element != null && !(element is PhoneTextBox))
            {
                element = VisualTreeHelper
                    .GetParent(element) as FrameworkElement;
            }

            if (element == null)
                return;

            var txtName = (PhoneTextBox)element;
            var field = (FieldBinding)txtName.Tag;
            field.IsEditing = false;
        }

        private void txtName_Changed(object sender, TextChangedEventArgs e)
        {
            var txtName = (PhoneTextBox)sender;
            var field = (FieldBinding)txtName.Tag;

            if (txtName.Text != field.Name)
                _entry.HasChanges = true;
        }

        private void txtName_KeyDown(object sender, KeyEventArgs e)
        {
            if (!e.IsEnter())
                return;

            var txtName = (PhoneTextBox)sender;
            var field = (FieldBinding)txtName.Tag;
            field.IsEditing = false;
        }

        private void txtName_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var txtName = (PhoneTextBox)sender;
            if (txtName.Visibility != Visibility.Visible)
                return;

            txtName.Focus();
            txtName.SelectAll();
        }
    }
}