using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Navigation;
using KeePass.Sources.DropBox.Api;
using KeePass.Utils;

namespace KeePass.Sources.DropBox
{
    public partial class List
    {
        private readonly ObservableCollection<MetaListItemInfo> _items;
        private string _path;
        private string _secret;
        private string _token;

        public List()
        {
            InitializeComponent();

            _items = new ObservableCollection<MetaListItemInfo>();
            lstBrowse.ItemsSource = _items;
        }

        protected override void OnNavigatedTo(
            bool cancelled, NavigationEventArgs e)
        {
            InitPars();

            if (_path != "/")
                PageTitle.Text = Path.GetFileName(_path);

            progList.IsLoading = true;
            new Client(_token, _secret)
                .ListAsync(_path, OnListComplete);
        }

        private void InitPars()
        {
            var pars = NavigationContext.QueryString;

            _path = pars["path"];
            _token = pars["token"];
            _secret = pars["secret"];
        }

        private void NavigateTo(string path)
        {
            this.NavigateTo<List>(
                "token={0}&secret={1}&path={2}",
                _token, _secret, path);
        }

        private void OnListComplete(MetaData data)
        {
            var dispatcher = Dispatcher;

            if (data.IsDir)
            {
                foreach (var child in data.Contents
                    .OrderBy(x => !x.IsDir)
                    .ThenBy(x => x.Name))
                {
                    var meta = child;
                    dispatcher.BeginInvoke(() => _items
                        .Add(new MetaListItemInfo(meta)));

                    Thread.Sleep(50);
                }
            }
            else
            {
                dispatcher.BeginInvoke(
                    GoBack<MainPage>);
            }

            dispatcher.BeginInvoke(() =>
                progList.IsLoading = false);
        }

        private void lstBrowse_SelectionChanged(
            object sender, SelectionChangedEventArgs e)
        {
            var meta = lstBrowse.SelectedItem as MetaListItemInfo;

            if (meta == null)
                return;

            NavigateTo(meta.Path);
            lstBrowse.SelectedItem = null;
        }
    }
}