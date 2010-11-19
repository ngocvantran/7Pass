using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Navigation;
using KeePass.Data;
using KeePass.IO;
using Microsoft.Phone.Shell;

namespace KeePass
{
    public partial class MainPage
    {
        private readonly DatabaseItems _items;

        public MainPage()
        {
            InitializeComponent();

            DataContext = _items =
                new DatabaseItems();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (LifeCycle.CheckDbState(this) ==
                DatabaseCheckResults.Terminate)
            {
                return;
            }

            var group = GetGroup();
            if (group == null)
            {
                this.OpenHome();
                return;
            }

            Display(group);

            var home = ((ApplicationBarIconButton)
                ApplicationBar.Buttons[0]);

            home.IsEnabled = NavigationContext
                .QueryString.Count != 0;
        }

        private void Display(Group root)
        {
            if (root == null)
                throw new ArgumentNullException("root");

            DisplayItems(root);
            PageTitle.Text = root.Name;
        }

        private void DisplayItems(Group root)
        {
            var converter = new ItemConverter();

            _items.Items = converter.Convert(root.Groups)
                .Union(converter.Convert(root.Entries))
                .ToList();
        }

        private Group GetGroup()
        {
            string groupId;
            var db = KeyCache.Database;
            var pars = NavigationContext.QueryString;

            return !pars.TryGetValue("id", out groupId)
                ? db.Root : db.GetGroup(new Guid(groupId));
        }

        private void Home_Click(object sender, EventArgs e)
        {
            this.OpenHome();
        }

        private void Search_Click(object sender, EventArgs e)
        {
            this.OpenSearch();
        }

        private void Settings_Click(object sender, EventArgs e)
        {
            this.OpenSettings();
        }

        private void lstItems_SelectionChanged(
            object sender, SelectionChangedEventArgs e)
        {
            var index = lstItems.SelectedIndex;
            if (index < 0)
                return;

            this.NavigateTo(_items.Items[index]);
        }
    }
}