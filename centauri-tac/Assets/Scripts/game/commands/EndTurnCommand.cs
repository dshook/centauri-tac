using strange.extensions.command.impl;
using ctac.signals;
using UnityEngine;
using System.Linq;

namespace ctac
{
    public class EndTurnCommand : Command
    {

        [Inject]
        public EndTurnSignal endTurn { get; set; }

        [Inject]
        public TurnEndedSignal turnEnded { get; set; }

        [Inject]
        public MinionsModel minionsModel { get; set; }

        [Inject]
        public GameTurnModel turnModel { get; set; }

        [Inject]
        public GamePlayersModel gamePlayers { get; set; }

        [Inject]
        public ISocketService socket { get; set; }

        [Inject]
        public IDebugService debug { get; set; }

        public override void Execute()
        {
            foreach (var minion in minionsModel.minions)
            {
                minion.hasMoved = false;
            }
            //hacktastic till turn comes from server
            turnModel.currentTurnClientId = gamePlayers
                .players
                .Where(x => x.clientId != turnModel.currentTurnClientId)
                .Select(x => x.clientId).FirstOrDefault();
            debug.Log("Turn Ended");
            turnEnded.Dispatch();
        }
    }
}

