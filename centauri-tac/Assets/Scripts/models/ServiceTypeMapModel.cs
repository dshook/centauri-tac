using ctac.signals;
using strange.extensions.signal.impl;
using System;
using System.Collections.Generic;

namespace ctac
{
    /// <summary>
    /// Maps string types for messages received through sockets to signal types
    /// </summary>
    [Singleton]
    public class ServiceTypeMapModel
    {
        private static Dictionary<string, Type> map = new Dictionary<string, Type>()
        {
            {"login", typeof(LoggedInSignal) },
            {"_ping", typeof(PingSignal) },
            {"_latency", typeof(LatencySignal) },
            {"token", typeof(TokenSignal) }
        };

        public Type Get(string def)
        {
            return map.Get(def);
        }
    }
}

