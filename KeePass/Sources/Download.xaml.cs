using System;
using System.Windows;
using KeePass.Storage;

namespace KeePass.Sources
{
    public partial class Download
    {
        public Download()
        {
            InitializeComponent();
        }

        private void lnkDemo_Click(object sender, RoutedEventArgs e)
        {
            var info = new DatabaseInfo();
            var demoDb = Application.GetResourceStream(
                new Uri("Sources/Demo7Pass.kdbx", UriKind.Relative));

            info.SetDatabase(demoDb.Stream, new DatabaseDetails
            {
                Source = "Demo",
                Name = "Demo Database",
            });

            MessageBox.Show(
                Properties.Resources.DemoDbText,
                Properties.Resources.DemoDbTitle,
                MessageBoxButton.OK);

            GoBack<MainPage>();
        }
    }
}