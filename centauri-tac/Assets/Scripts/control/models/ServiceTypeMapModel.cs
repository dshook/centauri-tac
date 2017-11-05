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
                {"socket:error", typeof(SocketErrorSignal) },
                {"socket:open", typeof(SocketConnectSignal) },
                {"socket:close", typeof(SocketCloseSignal) },
                {"socket:hangup", typeof(SocketHangupSignal) },

                {"login", typeof(ComponentLoggedInSignal) },
                {"me", typeof(PlayerFetchedSignal) },
                {"token", typeof(TokenSignal) },
                {"_ping", typeof(PingSignal) },
                {"_latency", typeof(LatencySignal) },

                {"qps", typeof(ServerQueueProcessStart) },
                {"qpc", typeof(ServerQueueProcessEnd) },
                {"game", typeof(GamelistGameSignal) },
                {"game:finished", typeof(ActionGameFinishedSignal) },
                {"game:current", typeof(CurrentGameSignal) },
                {"possibleActions", typeof(PossibleActionsSignal) },

                {"player:connect", typeof(PlayerConnectSignal) },
                {"player:join", typeof(PlayerJoinedSignal) },
                {"player:part", typeof(PlayerPartSignal) },
                {"player:disconnect", typeof(PlayerDisconnectSignal) },

                {"decks:current", typeof(GotDecksSignal) },
                {"decks:saveFailed", typeof(DeckSaveFailedSignal) },
                {"decks:saveSuccess", typeof(DeckSavedSignal) },

                {"status", typeof(MatchmakerStatusSignal) },
            }; 

            //auto add action bindings so action:name -> ActionNameSignal
            var assemblyTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.Name.StartsWith("Action") && !t.Name.Contains("ActionCancelled"));
            foreach (Type type in assemblyTypes)
            {
                var actionType = type.Name.Replace("Action", "").Replace("Signal", "");
                map.Add("action:" + actionType, type);
            }

            //auto add action cancelled bindings actionCancelled:name -> ActionCancelledNameSignal
            var assemblyCancelledTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.Name.StartsWith("ActionCancelled"));
            foreach (Type type in assemblyCancelledTypes)
            {
                var actionType = type.Name.Replace("ActionCancelled", "").Replace("Signal", "");
                map.Add("actionCancelled:" + actionType, type);
            }
        }

        public Type Get(string def)
        {
            return map.Get(def);
        }
    }
}

