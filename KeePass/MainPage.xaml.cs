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

            Display(GetGroup());

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
            var isDarkTheme = Utils.IsDarkTheme();
            var groupIcon = isDarkTheme
                ? "/Images/group.dark.png"
                : "/Images/group.light.png";
            var entryIcon = isDarkTheme
                ? "/Images/entry.dark.png"
                : "/Images/entry.light.png";

            _items.Items = root.Groups.Select(x =>
                new DatabaseItem
                {
                    Source = x,
                    Title = x.Name,
                    Icon = groupIcon,
                })
                .Union(root.Entries.Select(x =>
                    new DatabaseItem
                    {
                        Source = x,
                        Title = x.Title,
                        Icon = entryIcon,
                    }))
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

            var item = _items.Items[index].Source;

            var group = item as Group;
            if (group != null)
            {
                this.NavigateTo("/MainPage.xaml?id={0}", group.ID);
                return;
            }

            var entry = (Entry)item;
            this.NavigateTo("/EntryPage.xaml?id={0}", entry.ID);
        }
    }
}