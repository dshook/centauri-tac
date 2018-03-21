using System.Collections.Generic;
using UnityEngine;

namespace ctac
{
    public static class Constants
    {
        public const string playerToken = "playerToken";

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
            "playMinion","death","damaged","healed","attacks","ability","cardDrawn","turnEnd","turnStart","playSpell","cardDiscarded","chargeChange"
        };

        public static List<string> autoCardTags = new List<string>()
        {
            "Minion", "Ranged", "Melee"
        };

        public static Vector3 halfVector = new Vector3(0.5f, 0.5f, 0.5f);
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
            { Races.Vae, "UI/faction icons/faction_Venusians"},
            { Races.Earthlings, "UI/faction icons/faction_Earthlings" },
            { Races.Martians, "UI/faction icons/faction_Martians"},
            { Races.Grex, "UI/faction icons/faction_Grex"},
            { Races.Phaenon, "UI/faction icons/faction_Phaenon"},
            { Races.Lost, "UI/faction icons/faction_The Lost"},
        };

        public static Dictionary<string, string> keywordDescrips = new Dictionary<string, string>()
        {
            {"Aura",               "Keep this minion alive to gain the Aura effect" },
            {"Ability",            "Ability(X) Do something for X energy once per turn" },
            {"Bribe",              "Your opponent can pay the Bribe energy cost to charm this minion" },
            {"Synthesis",          "Effect activates when played" },
            {"Demise",             "Effect activates when the minion dies" },
            {"Volatile",           "Effect is active while the minion is damaged" },
            {"Spell Damage",       "Spells do one more damage for each spell damage point" },

            {"Silence",            "Negates a minions card text, abilities, and status effects" },
            {"Holtz Shield",       "Absorbes all damage a minion takes on the first hit" },
            {"Petrify",            "Character cannot move or attack" },
            {"Taunt",              "Minions entering the area of influence must attack this minion." },
            {"Cloak",              "Cannot be targeted until minion deals or takes damage" },
            {"Elusive",            "Cannot be targeted by spells, abilities, or hero powers" },
            {"Empower",            "If you have a charge, consume it and gain the effect" },
            {"Root",               "Cannot move" },
            {"Charge",             "Minion can move and attack immediately" },
            {"Dyad Strike",        "Minion can attack twice per turn" },
            {"Flying",             "Minion can fly up or down any height and over obstacles" },
            {"Airdrop",            "Minion can spawn in a larger area around your hero" },
            {"Piercing",           "This minion's attacks also hit the two characters behind its target" },
            {"Cleave",             "This minion's attacks also hit the two characters on either side of its target" },
        };

        public static Dictionary<Statuses, string> StatusParticleResources = new Dictionary<Statuses, string>()
        {
            { Statuses.Cloak, "Particles/Cloak Status"},
            { Statuses.hasAura, "Particles/Aura Status"},
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
