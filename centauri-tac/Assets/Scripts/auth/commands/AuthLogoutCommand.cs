using ctac.signals;
using strange.extensions.command.impl;
using UnityEngine;

namespace ctac
{
    public class AuthLogoutCommand : Command
    {
        [Inject] public ISocketService socketService { get; set; }

        [Inject] public NeedLoginSignal needLogin { get; set; }
        [Inject] public PlayersModel players { get; set; }

        public override void Execute()
        {
            foreach (var player in players.players)
            {
                socketService.Disconnect(player.clientId);
            }

            PlayerPrefs.SetString(Constants.playerToken, null);

            needLogin.Dispatch(null);
        }
    }
}

