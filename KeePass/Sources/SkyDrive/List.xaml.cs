using System;
using System.Windows.Navigation;
using Newtonsoft.Json;
using RestSharp;

namespace KeePass.Sources.SkyDrive
{
    public partial class List
    {
        private RestClient _client;

        public List()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(
            bool cancelled, NavigationEventArgs e)
        {
            if (cancelled)
                return;

            var token = NavigationContext
                .QueryString["token"];

            _client = new RestClient(
                "https://apis.live.net/v5.0/");
            _client.AddDefaultParameter("token",
                token, ParameterType.UrlSegment);

            DisplayEmail();
        }

        private void DisplayEmail()
        {
            var restRequest = new RestRequest(
                "me?access_token={token}")
            {
                RequestFormat = DataFormat.Json,
            };

            _client.ExecuteAsync(restRequest, x =>
            {
                var doc = JsonConvert.DeserializeXNode(
                    x.Content, "root");

                var email = doc.Root.GetValue(e => e.Value,
                    "emails", "preferred");

                if (!string.IsNullOrEmpty(email))
                {
                    Dispatcher.BeginInvoke(() =>
                        lblEmail.Text = email);
                }
            });
        }
    }
}