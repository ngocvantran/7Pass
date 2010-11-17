using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Navigation;
using KeePass.Data;
using KeePass.IO;

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
        }

        private void Display(Group root)
        {
            if (root == null)
                throw new ArgumentNullException("root");

            PageTitle.Text = root.Name;

            _items.Items = root.Groups.Select(x =>
                new DatabaseItem
                {
                    Source = x,
                    Title = x.Name,
                    Icon = "Images/warning.png",
                })
                .Union(root.Entries.Select(x =>
                    new DatabaseItem
                    {
                        Source = x,
                        Title = x.Title,
                        Icon = "Images/warning.png",
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

        private void OpenSettings(object sender, EventArgs e)
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