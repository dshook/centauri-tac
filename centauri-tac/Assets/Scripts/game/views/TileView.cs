using UnityEngine;
using ctac.util;
using System.Linq;
using SVGImporter;

namespace ctac {
    [SelectionBase]
    public class TileView : MonoBehaviour
    {
        public Tile tile;

        GameObject tileIndicatorGO;
        SVGRenderer tileIndicator;

        GameObject attackIndicatorGO;
        SVGRenderer attackIndicator;

        //these are just used for map editing and saving
        public bool unpassable = false;
        public bool clearable = false;
        public bool isStartTile = false;

        Color hoverTint = new Color(.2f, .2f, .2f, .2f);
        Color selectColor = new Color(.4f, .9f, .4f);
        //Color moveColor = new Color(.4f, .4f, .9f);
        Color moveRangeColor = ColorExtensions.HexToColor("D2D2FF");
        Color pathFindColor = ColorExtensions.HexToColor("7478FF");
        Color attackRangeColor = ColorExtensions.HexToColor("FF5656");
        Color attackColor = new Color(.9f, .4f, .4f);
        Color dimmedColor = new Color(.3f, .3f, .3f, .3f);
        Color disabledColor = ColorExtensions.HexToColor("4d4d4d");
        Color disabledAttackColor = ColorExtensions.HexToColor("6d6d6d");

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
                tileIndicatorGO = transform.Find("TileIndicator").gameObject;
                tileIndicator = tileIndicatorGO.gameObject.GetComponent<SVGRenderer>();

                attackIndicatorGO = transform.Find("AttackIndicator").gameObject;
                attackIndicator = attackIndicatorGO.gameObject.GetComponent<SVGRenderer>();
            }
        }


        Color tint = Colors.invisible;
        Color desiredColor = Colors.invisible;
        Color desiredAttackColor = Colors.invisible;

        void Update()
        {
            if (tile == null) return;
            
            //tileFlags = tile.highlightStatus;
            tint = Colors.invisible;
            desiredColor = Colors.invisible;
            desiredAttackColor = Colors.invisible;

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

            if((tile.highlightStatus & TileHighlightStatus.MoveRangeTotal) != 0)
            {
                desiredColor = disabledColor;
            }
            if ((tile.highlightStatus & TileHighlightStatus.MoveRange) != 0)
            {
                desiredColor = moveRangeColor;
            }
            if ((tile.highlightStatus & TileHighlightStatus.PathFind) != 0)
            {
                desiredColor = pathFindColor;
            }

            if ((tile.highlightStatus & TileHighlightStatus.AttackRangeTotal) != 0)
            {
                desiredAttackColor = disabledAttackColor;
            }
            if ((tile.highlightStatus & TileHighlightStatus.AttackRange) != 0)
            {
                desiredAttackColor = attackRangeColor;
            }
            if ((tile.highlightStatus & TileHighlightStatus.Attack) != 0)
            {
                desiredAttackColor = attackColor;
            }

            if (tile.highlightStatus == 0)
            {
                desiredColor = Colors.invisible;
            }

            if(desiredColor != Colors.invisible){
                tileIndicatorGO.SetActive(true);
                tileIndicator.color = desiredColor - tint;
            }else{
                tileIndicatorGO.SetActive(false);
            }

            if(desiredAttackColor != Colors.invisible){
                attackIndicatorGO.SetActive(true);
                attackIndicator.color = desiredAttackColor;
            }else{
                attackIndicatorGO.SetActive(false);
            }
        }
    }
}