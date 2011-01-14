using System;
using System.Collections.Generic;

namespace KeePass.Sources.DropBox.Api
{
    public class MetaData
    {
        public long Bytes { get; set; }

        public List<MetaData> Contents { get; set; }

        public string Hash { get; set; }

        public string Icon { get; set; }

        public bool IsDir { get; set; }

        public DateTime Modified { get; set; }

        public string Name
        {
            get
            {
                return System.IO.Path
                    .GetFileName(Path);
            }
        }

        public string Path { get; set; }

        public string Root { get; set; }

        public string Size { get; set; }

        public bool ThumbExists { get; set; }
    }
}