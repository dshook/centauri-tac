using System.Collections.Generic;
using UnityEngine;

namespace ctac
{
    public static class Constants
    {
        public static float hoverTime = 0.65f;
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


        public static Vector3 cardCircleCenter = new Vector3(0, -590, 332);
        public static Vector3 opponentCardCircleCenter = new Vector3(0, 750, 332);
        public static float cardCircleRadius = 480f;

        public static Dictionary<Races, string> RaceIconPaths = new Dictionary<Races, string>()
        {
            { Races.Neutral, "UI/faction icons/faction_Neutral"},
            { Races.Venusians, "UI/faction icons/faction_Venusians"},
            { Races.Earthlings, "UI/faction icons/faction_Earthlings" },
            { Races.Martians, "UI/faction icons/faction_Martians"},
            { Races.Grex, "UI/faction icons/faction_Grex"},
            { Races.Phaenon, "UI/faction icons/faction_Phaenon"},
            { Races.Lost, "UI/faction icons/faction_Lost"},
        };
    }

    public enum InjectionKeys
    {
        PersistentSignalsRoot,
        PiecesSignalsRoot,
        GameSignalsRoot,
        MainMenuSignalsRoot,
    }
}
