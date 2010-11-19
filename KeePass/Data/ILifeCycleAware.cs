using System;
using System.Collections.Generic;

namespace KeePass.Data
{
    internal interface ILifeCycleAware
    {
        void Load(IDictionary<string, object> states);
        void Save(IDictionary<string, object> states);
    }
}