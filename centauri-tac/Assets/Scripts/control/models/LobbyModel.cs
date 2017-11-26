using UnityEngine;

namespace ctac
{
    //Holds data that needs to be persisted across lobby scenes
    public class LobbyModel
    {
        public SocketKey lobbyKey = null; //holds the key to the lobby castle for making requests
        public Camera cardCamera = null;

        public Vector3 mainMenuPosition = new Vector3(0, 100, 60);
        public Vector3 cardsMenuPosition = new Vector3(225, 100, 60);
        public Vector3 playMenuPosition = new Vector3(0, 250, 60);
        public float menuTransitionTime = 0.8f;

    }
}

