using System.Collections.Generic;
using UnityEngine;

namespace ctac
{
    public enum LobbyScreens {
        main,
        cards,
        play,
        exchange
    }
    //Holds data that needs to be persisted across lobby scenes
    public class LobbyModel
    {
        public SocketKey lobbyKey = null; //holds the key to the lobby castle for making requests
        public Camera cardCamera = null;

        public static Dictionary<LobbyScreens, Vector3> lobbyPositions = new Dictionary<LobbyScreens, Vector3>()
        {
            { LobbyScreens.main, new Vector3(0, 100, 60) },
            { LobbyScreens.cards, new Vector3(225, 100, 60) },
            { LobbyScreens.play, new Vector3(0, 250, 60) },
            { LobbyScreens.exchange, new Vector3(0, -49, 60) },
        };

        public float menuTransitionTime = 0.8f;

    }
}

