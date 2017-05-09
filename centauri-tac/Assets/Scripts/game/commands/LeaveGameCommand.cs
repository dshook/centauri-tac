using strange.extensions.command.impl;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ctac
{
    public class LeaveGameCommand : Command
    {
        [Inject]
        public ISocketService socket { get; set; }

        [Inject]
        public SocketKey socketKey { get; set; }

        [Inject]
        public bool returnToMainMenu { get; set; }

        [Inject]
        public GamePlayersModel gamePlayers { get; set; }

        public override void Execute()
        {
            foreach (var player in gamePlayers.players)
            {
                var key = new SocketKey(player.clientId, "game");
                if (socket.IsSocketOpen(key))
                {
                    socket.Request(key, "part");
                }
            }

            if (!returnToMainMenu)
            {
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }
            else
            {
                SceneManager.LoadScene("main", LoadSceneMode.Single);
            }
        }
    }
}

