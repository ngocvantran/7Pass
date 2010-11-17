using System;
using System.Collections.Generic;

namespace KeePass.IO
{
    internal class GroupSorter : Comparer<Group>
    {
        public override int Compare(Group x, Group y)
        {
            return string.Compare(
                x.Name, y.Name);
        }
    }
}