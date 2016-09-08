using UnityEngine;
using ctac.util;

namespace ctac {
    public class TileView : MonoBehaviour
    {
        public Tile tile;
        public Color hoverTint = new Color(.1f, .1f, .1f, .1f);
        public Color pathFindTint = new Color(.3f, .3f, .3f, 1f);
        public Color friendlyTauntTint = new Color(.15f, .0f, .15f, .4f);
        public Color enemyTauntTint = new Color(0f, .15f, .0f, .4f);
        public Color selectColor = new Color(.4f, .9f, .4f);
        public Color moveColor = new Color(.4f, .4f, .9f);
        public Color moveRangeColor = new Color(1f, 1f, 1f, 0f);
        public Color attackRangeTint = new Color(1f, .1f, .1f);
        public Color attackColor = new Color(.9f, .4f, .4f);
        public Color dimmedColor = ColorExtensions.HexToColor("#aaaaaa");

        private MeshRenderer meshRenderer = null;
        private GameObject arrows = null;
        private Color invisible = new Color(0f, 0f, 0f, 0f);
        private Color tint;

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

            tint = invisible;
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
                SetColor(attackColor, tint);
            }
            if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.Selected)
                || FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.TargetTile))
            {
                SetColor(selectColor, tint);
            }
            if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.Movable))
            {
                SetColor(moveColor, tint);
            }
            if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.MoveRange))
            {
                SetColor(moveRangeColor, tint);
            }
            if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.AttackRange))
            {
                SetColor(attackRangeTint, tint);
            }

            if (tile.highlightStatus == TileHighlightStatus.None)
            {
                meshRenderer.material.color = originalColor;
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
            meshRenderer.material.color = originalColor - tint;
            return tint;
        }

        private void SetColor(Color c, Color tint)
        {
            meshRenderer.material.color = c - tint;
        }
    }
}