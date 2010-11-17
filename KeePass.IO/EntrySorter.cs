using System;
using System.Collections.Generic;

namespace KeePass.IO
{
    internal class EntrySorter : Comparer<Entry>
    {
        public override int Compare(Entry x, Entry y)
        {
            return string.Compare(
                x.Title, y.Title);
        }
    }
}