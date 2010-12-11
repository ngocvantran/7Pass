using System;
using System.Collections.Generic;

namespace KeePass.Services
{
    internal interface ILifeCycleAware
    {
        void Load(IDictionary<string, object> states);
        void Save(IDictionary<string, object> states);
    }
}