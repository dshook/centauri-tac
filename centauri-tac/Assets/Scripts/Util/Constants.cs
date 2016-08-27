using System.Collections.Generic;
using UnityEngine;

namespace ctac
{
    public static class Constants
    {
        public static float cameraRaycastDist = 150f;

        //can't move up more than this change in height
        public static float heightDeltaThreshold = 0.75f;

        public static string cardCanvas = "cardCanvas";
        public static string cardCamera = "CardCamera";
        public static string targetPieceTag = "targetPiece";
        public static string chooseCardTag = "chooseCard";
        public static string historyTileTag = "HistoryTile";

        public static Vector3 cardSpawnPosition = new Vector3(10000,10000, 0);

        public static List<string> eventTags = new List<string>()
        {
            "playMinion","death","damaged","healed","attacks","ability","cardDrawn","turnEnd","turnStart","playSpell","cardDiscarded"
        };
    }
}
