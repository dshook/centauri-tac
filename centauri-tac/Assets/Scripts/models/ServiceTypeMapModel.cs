using ctac.signals;
using strange.extensions.signal.impl;
using System;
using System.Collections.Generic;

namespace ctac
{
    /// <summary>
    /// Maps string types for messages received through sockets to signal types
    /// </summary>
    public interface IServiceTypeMapModel
    {
        Type Get(string def);
    }

    [Singleton]
    public class ServiceTypeMapModel : IServiceTypeMapModel
    {
        private static Dictionary<string, Type> map = new Dictionary<string, Type>()
        {
            {"login", typeof(LoggedInSignal) },
            {"token", typeof(TokenSignal) }
        };

        public Type Get(string def)
        {
            return map.Get(def);
        }
    }
}

