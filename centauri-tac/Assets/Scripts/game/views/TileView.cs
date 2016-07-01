using UnityEngine;

namespace ctac {
    public class TileView : MonoBehaviour
    {
        public Tile tile;
        public Color hoverTint = new Color(.1f, .1f, .1f, .1f);
        public Color pathFindTint = new Color(.3f, .3f, .3f, .3f);
        public Color attackRangeTint = new Color(.1f, .3f, .3f, .3f);
        public Color friendlyTauntTint = new Color(.15f, .0f, .15f, .2f);
        public Color enemyTauntTint = new Color(0f, .15f, .0f, .2f);
        public Color selectColor = new Color(.4f, .9f, .4f);
        public Color moveColor = new Color(.4f, .4f, .9f);
        public Color moveRangeColor = new Color(.6f, .6f, 1f);
        public Color attackColor = new Color(.9f, .4f, .4f);

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
                tint = friendlyTauntTint;
                meshRenderer.material.color = originalColor - tint;
            }
            if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.EnemyTauntArea))
            {
                tint = tint + enemyTauntTint;
                meshRenderer.material.color = originalColor - tint;
            }
            if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.AttackRange))
            {
                tint = tint + attackRangeTint;
                meshRenderer.material.color = originalColor - tint;
            }
            if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.Highlighted))
            {
                tint = tint + hoverTint;
                meshRenderer.material.color = originalColor - tint;
            }
            else if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.PathFind))
            {
                tint = tint + pathFindTint;
                meshRenderer.material.color = originalColor - tint;
            }

            if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.Attack))
            {
                meshRenderer.material.color = attackColor - tint;
            }
            if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.Selected)
                || FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.TargetTile))
            {
                meshRenderer.material.color = selectColor - tint;
            }
            if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.Movable))
            {
                meshRenderer.material.color = moveColor - tint;
            }
            if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.MoveRange))
            {
                meshRenderer.material.color = moveRangeColor - tint;
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
    }
}