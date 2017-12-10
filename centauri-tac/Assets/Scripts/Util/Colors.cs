using ctac.util;
using System.Collections.Generic;
using UnityEngine;

namespace ctac
{
    public static class Colors
    {
        public static Color32 white = Color.white;
        public static Color32 lightGray = ColorExtensions.HexToColor("#CCCCCC");
        public static Color32 darkGray = ColorExtensions.HexToColor("#555555");
        public static Color32 darkerGray = ColorExtensions.HexToColor("#333333");
        public static Color32 transparentWhite = new Color(1, 1, 1, 0);

        public static Color invisible = new Color(0f, 0f, 0f, 0f);

        public static Color32 friendlyColor = ColorExtensions.HexToColor("#0D80C5");
        public static Color32 enemyColor = ColorExtensions.HexToColor("#C52F0D");

        public static Color32 RarityCommon = ColorExtensions.HexToColor("#999999");
        public static Color32 RarityRare = ColorExtensions.HexToColor("#2598C5");
        public static Color32 RarityExotic = ColorExtensions.HexToColor("#64FF07");
        public static Color32 RarityAscendant = ColorExtensions.HexToColor("#FF0000");
        
        public static Color32 cardMetCondition = ColorExtensions.HexToColor("740000");

        public static Color32 numberSplatDamageColor = ColorExtensions.HexToColor("FD0000");
        public static Color32 numberSplatDamageColorBot = ColorExtensions.HexToColor("470000");
        public static Color32 numberSplatHealColor = ColorExtensions.HexToColor("00F00B");
        public static Color32 numberSplatHealColorBot = ColorExtensions.HexToColor("006504");

        public static Color32 targetOutlineColor = ColorExtensions.HexToColor("E1036C");
        public static Color32 moveAttackOutlineColor = ColorExtensions.HexToColor("63FF32");
        public static Color32 moveOutlineColor = ColorExtensions.HexToColor("006BFF");
        public static Color32 attackOutlineColor = ColorExtensions.HexToColor("FF5E2E");
        public static Color32 selectedOutlineColor = ColorExtensions.HexToColor("DBFF00");

        public static Color32 tauntEnemyColor = ColorExtensions.HexToColor("#E52600");
        public static Color32 tauntFriendlyColor = ColorExtensions.HexToColor("#0057E5");

        public static Color32 tileIndicatorFriendlyColor = friendlyColor;
        public static Color32 tileIndicatorEnemyColor = enemyColor;


        public static Dictionary<Races, Color32> RacePrimaries = new Dictionary<Races, Color32>()
        {
            { Races.Neutral, ColorExtensions.HexToColor("#000000") },
            { Races.Vae, ColorExtensions.HexToColor("FFEC2A") },
            { Races.Earthlings, ColorExtensions.HexToColor("#1100C5") },
            { Races.Martians, ColorExtensions.HexToColor("#EF3100") },
            { Races.Grex, ColorExtensions.HexToColor("#3F1D0B") },
            { Races.Phaenon, ColorExtensions.HexToColor("#961DDE") },
            { Races.Lost, ColorExtensions.HexToColor("#323063") },
        };

        public static Dictionary<Races, Color32> RaceSecondaries = new Dictionary<Races, Color32>()
        {
            { Races.Neutral, ColorExtensions.HexToColor("#808080") },
            { Races.Vae, ColorExtensions.HexToColor("#FFFFFF") },
            { Races.Earthlings, ColorExtensions.HexToColor("#238B00") },
            { Races.Martians, ColorExtensions.HexToColor("#FBA700") },
            { Races.Grex, ColorExtensions.HexToColor("#16B063") },
            { Races.Phaenon, ColorExtensions.HexToColor("#FFD81B") },
            { Races.Lost, ColorExtensions.HexToColor("#692AFF") },
        };
    }
}
