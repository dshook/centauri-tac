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

#pragma warning disable
        [SerializeField]
        [EnumFlagsAttribute] TileHighlightStatus tileFlags;
#pragma warning restore

        //private MeshRenderer meshRenderer = null;
        //private GameObject arrows = null;

        public void Init()
        {
            if (tile != null)
            {
                var indicator = transform.FindChild("TileIndicator").gameObject;
                tileIndicator = indicator.gameObject.GetComponent<SVGRenderer>();
                //arrows = tile.gameObject.transform.FindChild("Arrows").gameObject;
            }
        }

        void Update()
        {
            if (tile == null) return;
            
            tileFlags = tile.highlightStatus;
            Color tint = Colors.invisible;
            Color desiredColor = Colors.invisible;

            if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.Dimmed))
            {
                tint += dimmedColor;
            }
            if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.Highlighted))
            {
                tint += hoverTint;
            }

            if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.Selected)
                || FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.TargetTile))
            {
                desiredColor = selectColor;
            }
            if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.Movable))
            {
                desiredColor = moveColor;
            }
            if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.MoveRange))
            {
                desiredColor = moveRangeColor;
            }
            if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.PathFind))
            {
                desiredColor = pathFindColor;
            }
            if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.Attack))
            {
                desiredColor = attackColor;
            }
            if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.AttackRange))
            {
                desiredColor = attackRangeTint;
            }

            if (tile.highlightStatus == 0)
            {
                desiredColor = Colors.invisible;
            }

            tileIndicator.color = desiredColor - tint;
            //for tint only color changes just set the color, don't tween
            //if (desiredColor == originalColor && tint != invisible)
            //{
            //}
            //else
            //{
            //    desiredColor = desiredColor - tint;

            //    ReallySetColor(desiredColor);

                //if (desiredColor != meshRenderer.material.color)
                //{
                //    var existingTweens = gameObject.GetComponents<MaterialColorTween>();
                //    if (existingTweens.Length == 0 || !existingTweens.Any(tw => tw.desiredColor == desiredColor))
                //    {
                //        var matTween = gameObject.AddComponent<MaterialColorTween>();
                //        matTween.mat = meshRenderer.material;
                //        matTween.desiredColor = desiredColor;
                //        matTween.time = 0.5f;
                //    }
                //}
            //}

            //if (tile.showPieceRotation && !arrows.activeSelf)
            //{
            //    arrows.SetActive(true);
            //}
            //else if(!tile.showPieceRotation && arrows.activeSelf)
            //{
            //    arrows.SetActive(false);
            //}
        }

        //private void ReallySetColor(Color c)
        //{
        //    meshRenderer.material.SetFloat("_UseColor", 0.8f);
        //    meshRenderer.material.SetColor("_Color", c);
        //}

        //private void ResetColor()
        //{
        //    meshRenderer.material.SetFloat("_UseColor", 0);
        //    meshRenderer.material.SetColor("_Color", Color.white);
        //}
    }
}