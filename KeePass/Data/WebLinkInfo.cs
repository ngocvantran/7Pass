using System;
using System.IO;

namespace KeePass.Data
{
    public class WebLinkInfo
    {
        private readonly string _name;
        private readonly string _url;

        public string Name
        {
            get { return _name; }
        }

        public string Url
        {
            get { return _url; }
        }

        public WebLinkInfo(string url)
        {
            _url = url;

            try
            {
                _name = Path.GetFileName(url);
            }
            catch (ArgumentException)
            {
                _name = string.Empty;
            }
        }
    }
}