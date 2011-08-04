using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using KeePass.Data;
using KeePass.Generator;
using KeePass.IO.Data;
using KeePass.Storage;
using KeePass.Utils;

namespace KeePass
{
    public partial class PassGen
    {
        private const int MIN_LENGTH = 4;
        private readonly Timer _tmrUpdate;

        private CharacterSetCheckBox[] _checks;
        private Entry _entry;

        public PassGen()
        {
            InitializeComponent();

            _tmrUpdate = new Timer(
                _ => UpdateSettings());
            LayoutUpdated += OnLayoutUpdated;
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

            var id = NavigationContext
                .QueryString["id"];

            _entry = database.GetEntry(id)
                ?? CurrentEntry.Entry;

            LoadCurrentState();

            TriggerUpdate();
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
                check.Checked += OnSettingsChanged;
                check.Unchecked += OnSettingsChanged;
                pnlSets.Children.Add(check);
            }

            sldLength.ValueChanged += OnSettingsChanged;
        }

        private void TriggerUpdate()
        {
            _tmrUpdate.Change(1000,
                Timeout.Infinite);
        }

        private void UpdateSettings()
        {
            var dispatcher = Dispatcher;

            dispatcher.BeginInvoke(() =>
            {
                var length = (int)sldLength.Value;

                var characters = _checks
                    .Where(x => x.IsChecked != null &&
                        x.IsChecked.Value)
                    .SelectMany(x => x.Characters)
                    .ToArray();

                ThreadPool.QueueUserWorkItem(_ =>
                {
                    var results = new GenerationResults(
                        characters, length);

                    dispatcher.BeginInvoke(() =>
                        lstResults.ItemsSource = results);
                });
            });
        }

        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            LayoutUpdated -= OnLayoutUpdated;
            TriggerUpdate();
        }

        private void OnSettingsChanged(object sender, EventArgs e)
        {
            TriggerUpdate();
        }

        private void cmdAbout_Click(object sender, EventArgs e)
        {
            this.NavigateTo<About>();
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

            CurrentEntry.HasChanges = true;
            _entry.Password = item.Password;

            NavigationService.GoBack();
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