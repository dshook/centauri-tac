using ctac.util;
using System.Collections.Generic;
using UnityEngine;

namespace ctac
{
    public static class Colors
    {
        public static Color32 friendlyColor = ColorExtensions.HexToColor("#65ACFF");
        public static Color32 enemyColor = ColorExtensions.HexToColor("#FF4949");

        public static Color32 RarityCommon = ColorExtensions.HexToColor("#999999");
        public static Color32 RarityRare = ColorExtensions.HexToColor("#2598C5");
        public static Color32 RarityExotic = ColorExtensions.HexToColor("#64FF07");
        public static Color32 RarityMythical = ColorExtensions.HexToColor("#FF0000");

        public static Dictionary<Races, Color32> RacePrimaries = new Dictionary<Races, Color32>()
        {
            { Races.Neutral, ColorExtensions.HexToColor("#000000") },
            { Races.Venusians, ColorExtensions.HexToColor("FFEC2A") },
            { Races.Earthlings, ColorExtensions.HexToColor("#1100C5") },
            { Races.Martians, ColorExtensions.HexToColor("#EF3100") },
            { Races.Grex, ColorExtensions.HexToColor("#3F1D0B") },
            { Races.Phaenon, ColorExtensions.HexToColor("#961DDE") },
            { Races.Lost, ColorExtensions.HexToColor("#323063") },
        };

        public static Dictionary<Races, Color32> RaceSecondaries = new Dictionary<Races, Color32>()
        {
            { Races.Neutral, ColorExtensions.HexToColor("#808080") },
            { Races.Venusians, ColorExtensions.HexToColor("#FFFFFF") },
            { Races.Earthlings, ColorExtensions.HexToColor("#238B00") },
            { Races.Martians, ColorExtensions.HexToColor("#FBA700") },
            { Races.Grex, ColorExtensions.HexToColor("#16B063") },
            { Races.Phaenon, ColorExtensions.HexToColor("#FFD81B") },
            { Races.Lost, ColorExtensions.HexToColor("#692AFF") },
        };
    }
}
