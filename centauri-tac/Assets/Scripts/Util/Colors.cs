using ctac.util;
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
    }
}
