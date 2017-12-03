using System.Collections.Generic;
using UnityEngine;

namespace ctac
{
    public static class Constants
    {
        public static float hoverTime = 0.45f;
        public const float dragDistThreshold = 10f;
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

        public static Vector2 centerAnchor = new Vector2(0.5f, 0.5f);
        public static Vector2 topLeftAnchor = new Vector2(0, 1);
        public static Vector2 topLeftCardOffset = new Vector2(-20f, 0f);
        public static float cardHoverZPos = 100f;

        public static Dictionary<Races, string> RaceIconPaths = new Dictionary<Races, string>()
        {
            { Races.Neutral, "UI/faction icons/faction_Neutral"},
            { Races.Venusians, "UI/faction icons/faction_Venusians"},
            { Races.Earthlings, "UI/faction icons/faction_Earthlings" },
            { Races.Martians, "UI/faction icons/faction_Martians"},
            { Races.Grex, "UI/faction icons/faction_Grex"},
            { Races.Phaenon, "UI/faction icons/faction_Phaenon"},
            { Races.Lost, "UI/faction icons/faction_The Lost"},
        };

        public static Dictionary<string, string> keywordDescrips = new Dictionary<string, string>()
        {
            {"Ability",            "Active ability to use with energy cost in parenthesis" },
            {"Synthesis",          "Does something when played" },
            {"Demise",             "Does something when the minion dies" },
            {"Volatile",           "Does something when damaged" },
            {"Spell Damage",       "Spells do one more damage for each spell damage point" },

            {"Silence",            "Negates a minions card text, abilities, and status effects" },
            {"Holtz Shield",       "Absorbes all damage a minion takes on the first hit" },
            {"Paralyze",           "Minion cannot move or attack" },
            {"Taunt",              "Minions entering the area of influence must attack this minion." },
            {"Cloak",              "Cannot be targeted until minion deals or takes damage" },
            {"Tech Resist",        "Can't be targeted by spells, abilities, or hero powers" },
            {"Root",               "Cannot move" },
            {"Charge",             "Minion can move and attack immediately" },
            {"Dyad Strike",        "Minion can attack twice per turn" },
            {"Flying",             "Minion can fly up or down any height and over obstacles" },
            {"Airdrop",            "Minion can spawn in a larger area around your hero" },
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
