using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Navigation;
using KeePass.IO;

namespace KeePass
{
    public partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            using (var store = IsolatedStorageFile
                .GetUserStoreForApplication())
            {
                if (!store.FileExists(Consts.FILE_NAME))
                {
                    this.OpenDownload();
                    return;
                }

                if (!KeyCache.HasPassword)
                {
                    this.NavigateTo("/Password.xaml");
                    return;
                }

                if (KeyCache.Database == null)
                {
                    using (var fs = store.OpenFile(
                        Consts.FILE_NAME, FileMode.Open))
                    {
                        var reader = new DatabaseReader();

                        var root = reader.Load(fs,
                            KeyCache.Password);

                        KeyCache.Database = root;
                    }
                }

                using (var fs = store.OpenFile(
                    Consts.FILE_NAME, FileMode.Open))
                {
                    var reader = new DatabaseReader();

                    var root = reader.Load(fs,
                        KeyCache.Password);

                    Display(root);
                }

                Display(GetGroup());
            }
        }

        private void Display(Group root)
        {
            if (root == null)
                throw new ArgumentNullException("root");

            PageTitle.Text = root.Name;

            lstItems.Items.Clear();
            foreach (var group in root.Groups)
            {
                var item = new ListBoxItem();
                item.Content = group.Name;

                lstItems.Items.Add(item);
            }
        }

        private Group GetGroup()
        {
            string groupId;
            var root = KeyCache.Database;
            var pars = NavigationContext.QueryString;

            return !pars.TryGetValue("g", out groupId)
                ? root : GetGroup(root, new Guid(groupId));
        }

        private static Group GetGroup(Group group, Guid id)
        {
            if (group.ID == id)
                return group;

            return group.Groups
                .Select(x => GetGroup(x, id))
                .Where(x => x != null)
                .FirstOrDefault();
        }

        private void OpenSettings(object sender, EventArgs e)
        {
            this.OpenSettings();
        }
    }
}