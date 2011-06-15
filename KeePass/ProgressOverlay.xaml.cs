using System;
using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace KeePass
{
    public partial class ProgressOverlay
    {
        private bool? _appBarVisible;
        private bool _isBusy;

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                if (value == _isBusy)
                    return;

                _isBusy = value;
                progBusy.IsIndeterminate = value;

                pnlMain.IsEnabled = value;
                pnlMain.Visibility = value
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                var bar = GetAppBar();

                if (value)
                {
                    if (bar != null)
                    {
                        _appBarVisible = bar.IsVisible;
                        bar.IsVisible = false;
                    }
                    else
                        _appBarVisible = null;

                    pnlMain.Focus();
                }
                else if (_appBarVisible != null)
                {
                    if (bar != null)
                    {
                        bar.IsVisible =
                            _appBarVisible.Value;
                    }

                    _appBarVisible = null;
                }
            }
        }

        public string Text
        {
            get { return lblText.Text; }
            set { lblText.Text = value; }
        }

        public ProgressOverlay()
        {
            InitializeComponent();
        }

        private IApplicationBar GetAppBar()
        {
            var parent = Parent;

            while (parent != null)
            {
                var page = parent as PhoneApplicationPage;
                if (page != null)
                    return page.ApplicationBar;

                parent = ((FrameworkElement)parent).Parent;
            }

            return null;
        }
    }
}