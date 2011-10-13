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
                new PropertyMetadata(OnIsProtectedChanged));

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

        private void UpdateProtectState()
        {
            if (!IsProtected)
            {
                txtMask.Visibility = Visibility.Collapsed;
                return;
            }

            var focused = FocusManager.GetFocusedElement();
            txtMask.Visibility = ReferenceEquals(focused, txtPassword)
                ? Visibility.Collapsed : Visibility.Visible;
        }

        private void OnFocusChanged(object sender, RoutedEventArgs e)
        {
            UpdateProtectState();
        }

        private void txtMask_Loaded(object sender, RoutedEventArgs e)
        {
            var brush = txtMask.Background as SolidColorBrush;
            if (brush == null)
                return;

            brush.Opacity = 1;

            var color = brush.Color;
            brush.Color = Color.FromArgb(byte.MaxValue,
                color.R, color.G, color.B);
        }

        private void txtPassword_TextChanged(
            object sender, TextChangedEventArgs e)
        {
            OnTextChanged(e);
        }
    }
}