using System;

namespace KeePass.Sources.DropBox
{
    internal interface IListItem
    {
        bool IsDir { get; }
        string Path { get; }
    }
}