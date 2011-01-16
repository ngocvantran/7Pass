using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using KeePass.Utils;
using Microsoft.Phone.Tasks;

namespace KeePass
{
    public partial class TrialNotification
    {
        private readonly DispatcherTimer _tmrHide;
        private readonly DispatcherTimer _tmrShow;

        public TrialNotification()
        {
            InitializeComponent();

            _tmrShow = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3),
            };
            _tmrShow.Tick += _tmrShow_Tick;

            _tmrHide = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(10),
            };
            _tmrHide.Tick += _tmrHide_Tick;
        }

        private void _tmrHide_Tick(object sender, EventArgs e)
        {
            _tmrHide.Stop();

            var story = (Storyboard)Resources["hideBoard"];
            story.Begin();
        }

        private void _tmrShow_Tick(object sender, EventArgs e)
        {
            _tmrShow.Stop();
            popupTrial.IsOpen = true;
            TrialManager.PopupShown();

            _tmrHide.Start();
        }

        private void pnlMain_ManipulationStarted(
            object sender, ManipulationStartedEventArgs e)
        {
            e.Handled = true;
            new MarketplaceDetailTask().Show();
        }

        private void popupTrial_Loaded(object sender, RoutedEventArgs e)
        {
            _tmrShow.Start();
        }

        private void story_Completed(object sender, EventArgs e)
        {
            popupTrial.IsOpen = false;
        }
    }
}