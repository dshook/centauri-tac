using UnityEngine;
using ctac.util;

namespace ctac {
    public class TileView : MonoBehaviour
    {
        public Tile tile;
        public Color hoverTint = new Color(.1f, .1f, .1f, .1f);
        public Color pathFindTint = new Color(.3f, .3f, .3f, 1f);
        public Color friendlyTauntTint = ColorExtensions.HexToColor("F9A8FF");
        public Color enemyTauntTint = ColorExtensions.HexToColor("D2FFD1");
        public Color selectColor = new Color(.4f, .9f, .4f);
        public Color moveColor = new Color(.4f, .4f, .9f);
        public Color moveRangeColor = new Color(1f, 1f, 1f, 0f);
        public Color attackRangeTint = new Color(1f, .1f, .1f);
        public Color attackColor = new Color(.9f, .4f, .4f);
        public Color dimmedColor = ColorExtensions.HexToColor("#aaaaaa");

        [SerializeField]
        [EnumFlagsAttribute] TileHighlightStatus tileFlags;

        private MeshRenderer meshRenderer = null;
        private GameObject arrows = null;
        private Color invisible = new Color(0f, 0f, 0f, 0f);
        private Color tint;
        private Color color;

        private Color originalColor;

        void Start()
        {
            if (tile != null)
            {
                meshRenderer = tile.gameObject.GetComponentInChildren<MeshRenderer>();
                arrows = tile.gameObject.transform.FindChild("Arrows").gameObject;
                originalColor = meshRenderer.material.color;
            }
        }

        void Update()
        {
            if (tile == null) return;

            tileFlags = tile.highlightStatus;
            tint = invisible;
            color = invisible;
            if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.FriendlyTauntArea))
            {
                tint = UpdateAdditive(friendlyTauntTint, tint);
            }
            if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.EnemyTauntArea))
            {
                tint = UpdateAdditive(enemyTauntTint, tint);
            }
            if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.Dimmed))
            {
                tint = UpdateAdditive(dimmedColor, tint);
            }
            if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.Highlighted))
            {
                tint = UpdateAdditive(hoverTint, tint);
            }
            else if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.PathFind))
            {
                tint = UpdateAdditive(pathFindTint, tint);
            }

            if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.Attack))
            {
                color = attackColor;
            }
            if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.Selected)
                || FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.TargetTile))
            {
                color = selectColor;
            }
            if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.Movable))
            {
                color = moveColor;
            }
            if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.MoveRange))
            {
                color = moveRangeColor;
            }
            if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.AttackRange))
            {
                color = attackRangeTint;
            }

            if (tile.highlightStatus == 0)
            {
                ResetColor();
            }

            if (color != invisible)
            {
                ReallySetColor(color - tint);
            }
            else
            {
                if (tint != invisible) {
                    //only tint color
                    ReallySetColor(Color.white - tint);
                }
            }

            if (tile.showPieceRotation)
            {
                arrows.SetActive(true);
            }
            else
            {
                arrows.SetActive(false);
            }
        }

        private Color UpdateAdditive(Color c, Color tint)
        {
            tint = tint + c;
            //meshRenderer.material.color = Color.Lerp(originalColor, tint, 0.8f);
            return tint;
        }

        //private void SetColor(Color c, Color tint)
        //{
        //    meshRenderer.material.color = c - tint;
        //}

        private void ReallySetColor(Color c)
        {
            meshRenderer.material.SetFloat("_UseColor", 0.8f);
            meshRenderer.material.SetColor("_Color", c);
        }

        private void ResetColor()
        {
            meshRenderer.material.SetFloat("_UseColor", 0);
            meshRenderer.material.SetColor("_Color", Color.white);
        }
    }
}