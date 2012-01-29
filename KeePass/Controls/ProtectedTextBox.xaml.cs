using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace KeePass.Controls
{
    public partial class ProtectedTextBox
    {
        public static DependencyProperty IsProtectedProperty = DependencyProperty
            .Register("IsProtected", typeof(bool), typeof(ProtectedTextBox),
                new PropertyMetadata(true, OnIsProtectedChanged));

        public static DependencyProperty MonoSpacedProperty = DependencyProperty
            .Register("MonoSpaced", typeof(bool), typeof(ProtectedTextBox),
                new PropertyMetadata(false, OnMonoSpacedChanged));

        public static DependencyProperty TextProperty = DependencyProperty
            .Register("Text", typeof(string), typeof(ProtectedTextBox), null);

        /// <summary>
        /// Occurs when value of <see cref="Text"/> has changed.
        /// </summary>
        public event TextChangedEventHandler TextChanged;

        public bool IsProtected
        {
            get { return (bool)GetValue(IsProtectedProperty); }
            set { SetValue(IsProtectedProperty, value); }
        }

        public bool MonoSpaced
        {
            get { return (bool)GetValue(MonoSpacedProperty); }
            set { SetValue(MonoSpacedProperty, value); }
        }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>
        /// The text.
        /// </value>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public ProtectedTextBox()
        {
            InitializeComponent();
            UpdateProtectState();

            txtMask.DataContext = this;
            txtPassword.DataContext = this;
        }

        public void SelectAll()
        {
            txtPassword.SelectAll();
        }

        /// <summary>
        /// Raises the <see cref="TextChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="TextChangedEventArgs"/>
        /// instance containing the event data.</param>
        protected virtual void OnTextChanged(TextChangedEventArgs e)
        {
            if (TextChanged != null)
                TextChanged(this, e);
        }

        private static void OnIsProtectedChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var protect = (ProtectedTextBox)d;
            protect.UpdateProtectState();
        }

        private static void OnMonoSpacedChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var protect = (ProtectedTextBox)d;

            if (protect.MonoSpaced)
            {
                protect.txtPassword.FontFamily =
                    new FontFamily("Courier New");
            }
        }

        private void UpdateProtectState()
        {
            if (!IsProtected)
            {
                txtMask.Opacity = 0;
                txtPassword.Opacity = 1;

                return;
            }

            var focused = FocusManager.GetFocusedElement();
            var editing = ReferenceEquals(focused, txtPassword);

            txtMask.Opacity = editing ? 0 : 1;
            txtPassword.Opacity = editing ? 1 : 0;
        }

        private void txtPassword_GotFocus(object sender, RoutedEventArgs e)
        {
            txtPassword.Opacity = 1;
            UpdateProtectState();
        }

        private void txtPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            if (IsProtected)
                txtPassword.Opacity = 0;

            UpdateProtectState();
        }

        private void txtPassword_TextChanged(
            object sender, TextChangedEventArgs e)
        {
            OnTextChanged(e);
        }
    }
}