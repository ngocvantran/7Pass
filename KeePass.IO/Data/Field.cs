using System;
using System.Diagnostics;

namespace KeePass.IO.Data
{
    [DebuggerDisplay("Field {Name}")]
    public class Field
    {
        public string Name { get; set; }

        public bool Protected { get; set; }

        public string Value { get; set; }
    }
}