using UnityEngine;
using ctac.util;
using System.Linq;
using SVGImporter;

namespace ctac {
    [SelectionBase]
    public class TileView : MonoBehaviour
    {
        public Tile tile;
        private SVGRenderer tileIndicator;

        //these are just used for map editing and saving
        public bool unpassable = false;
        public bool clearable = false;
        public bool isStartTile = false;

        Color hoverTint = new Color(.2f, .2f, .2f, .2f);
        Color selectColor = new Color(.4f, .9f, .4f);
        Color moveColor = new Color(.4f, .4f, .9f);
        Color moveRangeColor = ColorExtensions.HexToColor("D2D2FF");
        Color pathFindColor = ColorExtensions.HexToColor("7478FF");
        Color attackRangeTint = new Color(1f, .1f, .1f);
        Color attackColor = new Color(.9f, .4f, .4f);
        Color dimmedColor = new Color(.3f, .3f, .3f, .3f);

//#pragma warning disable
//        [SerializeField]
//        [EnumFlagsAttribute] TileHighlightStatus tileFlags;
//#pragma warning restore

        //private MeshRenderer meshRenderer = null;
        //private GameObject arrows = null;

        public void Init()
        {
            if (tile != null)
            {
                var indicator = transform.Find("TileIndicator").gameObject;
                tileIndicator = indicator.gameObject.GetComponent<SVGRenderer>();
                //arrows = tile.gameObject.transform.FindChild("Arrows").gameObject;
            }
        }


        Color tint = Colors.invisible;
        Color desiredColor = Colors.invisible;

        void Update()
        {
            if (tile == null) return;
            
            //tileFlags = tile.highlightStatus;
            tint = Colors.invisible;
            desiredColor = Colors.invisible;

            if ((tile.highlightStatus & TileHighlightStatus.Dimmed) != 0)
            {
                tint += dimmedColor;
            }
            if ((tile.highlightStatus & TileHighlightStatus.Highlighted) != 0)
            {
                tint += hoverTint;
            }

            if ((tile.highlightStatus & TileHighlightStatus.Selected) != 0
                || (tile.highlightStatus & TileHighlightStatus.TargetTile) != 0)
            {
                desiredColor = selectColor;
            }
            if ((tile.highlightStatus & TileHighlightStatus.Movable) != 0)
            {
                desiredColor = moveColor;
            }
            if ((tile.highlightStatus & TileHighlightStatus.MoveRange) != 0)
            {
                desiredColor = moveRangeColor;
            }
            if ((tile.highlightStatus & TileHighlightStatus.PathFind) != 0)
            {
                desiredColor = pathFindColor;
            }
            if ((tile.highlightStatus & TileHighlightStatus.Attack) != 0)
            {
                desiredColor = attackColor;
            }
            if ((tile.highlightStatus & TileHighlightStatus.AttackRange) != 0)
            {
                desiredColor = attackRangeTint;
            }

            if (tile.highlightStatus == 0)
            {
                desiredColor = Colors.invisible;
            }

            tileIndicator.color = desiredColor - tint;
        }
    }
}