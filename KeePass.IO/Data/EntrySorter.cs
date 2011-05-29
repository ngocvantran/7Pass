using System;
using System.Collections.Generic;

namespace KeePass.IO.Data
{
    internal class EntrySorter : Comparer<Entry>
    {
        public override int Compare(Entry x, Entry y)
        {
            if (x == null && y == null)
                return 0;

            return string.Compare(
                x.Title, y.Title);
        }
    }
}