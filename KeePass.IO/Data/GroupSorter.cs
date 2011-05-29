using System;
using System.Collections.Generic;

namespace KeePass.IO.Data
{
    internal class GroupSorter : Comparer<Group>
    {
        public override int Compare(Group x, Group y)
        {
            if (x == null && y == null)
                return 0;

            return string.Compare(
                x.Name, y.Name);
        }
    }
}