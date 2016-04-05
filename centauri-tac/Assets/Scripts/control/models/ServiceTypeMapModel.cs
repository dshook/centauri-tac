using ctac.signals;
using strange.extensions.signal.impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ctac
{
    /// <summary>
    /// Maps string types for messages received through sockets to signal types
    /// </summary>
    [Singleton]
    public class ServiceTypeMapModel
    {
        private Dictionary<string, Type> map;
        public ServiceTypeMapModel()
        {
            map = new Dictionary<string, Type>()
            {
                {"login", typeof(LoggedInSignal) },
                {"me", typeof(PlayerFetchedSignal) },
                {"token", typeof(TokenSignal) },
                {"_ping", typeof(PingSignal) },
                {"_latency", typeof(LatencySignal) },

                {"qps", typeof(ServerQueueProcessStart) },
                {"qpc", typeof(ServerQueueProcessEnd) },
                {"game", typeof(GamelistGameSignal) },
                {"game:finished", typeof(GameFinishedSignal) },
                {"game:current", typeof(CurrentGameSignal) },
                {"possibleActions", typeof(PossibleActionsSignal) },

                {"player:connect", typeof(PlayerConnectSignal) },
                {"player:join", typeof(PlayerJoinedSignal) },
                {"player:part", typeof(PlayerPartSignal) },
                {"player:disconnect", typeof(PlayerDisconnectSignal) },

                {"status", typeof(MatchmakerStatusSignal) },

                {"actionCancelled:SpawnPiece", typeof(ActionSpawnPieceCancelledSignal) },
                {"actionCancelled:ActivateCard", typeof(ActionActivateCardCancelledSignal) },
            }; 

            //auto add action bindings
            var assemblyTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Name.StartsWith("Action"));
            foreach (Type type in assemblyTypes)
            {
                var actionType = type.Name.Replace("Action", "").Replace("Signal", "");
                map.Add("action:" + actionType, type);
            }
        }

        public Type Get(string def)
        {
            return map.Get(def);
        }
    }
}

