using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using KeePass.Data;
using KeePass.Generator;
using KeePass.Storage;
using KeePass.Utils;

namespace KeePass
{
    public partial class PassGen
    {
        private const int MIN_LENGTH = 4;

        private CharacterSetCheckBox[] _checks;
        private EntryBinding _entry;

        public PassGen()
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

            LoadCurrentState();
        }

        private void LoadCurrentState()
        {
            var password = _entry.Password;
            if (string.IsNullOrEmpty(password))
            {
                /* Default password:
                 * - Length = 20
                 * - Upper-case
                 * - Lower-case
                 * - Digits
                 */

                password = "Aa345678901234567890";
            }

            sldLength.Value = Math.Max(MIN_LENGTH,
                Math.Min(sldLength.Maximum, password.Length));

            _checks = CharacterSets
                .GetAll()
                .Select(x => new CharacterSetCheckBox(x))
                .ToArray();

            foreach (var check in _checks)
            {
                check.LoadState(password);
                pnlSets.Children.Add(check);
            }
        }

        private void TriggerUpdate()
        {
            Dispatcher.BeginInvoke(
                UpdateSettings);
        }

        private void UpdateSettings()
        {
            var length = (int)sldLength.Value;

            var characters = _checks
                .Where(x => x.IsChecked != null &&
                    x.IsChecked.Value)
                .SelectMany(x => x.Characters)
                .ToArray();

            var results = new GenerationResults(
                characters, length);

            lstResults.ItemsSource = results;
        }

        private void cmdAbout_Click(object sender, EventArgs e)
        {
            this.NavigateTo<About>();
        }

        private void cmdGenerate_Click(object sender, EventArgs e)
        {
            if (panMain.SelectedItem != panResults)
                panMain.DefaultItem = panResults;
            else
                TriggerUpdate();
        }

        private void cmdHome_Click(object sender, EventArgs e)
        {
            this.BackToRoot();
        }

        private void cmdRoot_Click(object sender, EventArgs e)
        {
            this.BackToDBs();
        }

        private void lstResults_SelectionChanged(
            object sender, SelectionChangedEventArgs e)
        {
            var item = lstResults.SelectedItem as
                GenerationResults.DataItem;

            if (item == null)
                return;

            _entry.HasChanges = true;
            _entry.Password = item.Password;

            NavigationService.GoBack();
        }

        private void panMain_SelectionChanged(
            object sender, SelectionChangedEventArgs e)
        {
            if (panMain.SelectedItem == panResults)
                TriggerUpdate();
        }

        private void sldLength_ValueChanged(object sender,
            RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue < 4)
            {
                sldLength.Value = 4;
                return;
            }

            var round = Math.Round(e.NewValue);
            if (e.NewValue != round)
            {
                sldLength.Value = round;
                return;
            }

            if (lblLength != null)
                lblLength.Text = round.ToString();
        }
    }
}