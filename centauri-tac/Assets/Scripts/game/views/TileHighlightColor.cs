using UnityEngine;

namespace ctac {
    public class TileHighlightColor : MonoBehaviour
    {
        public Tile tile;
        public Color hoverTint = new Color(.1f, .1f, .1f, .1f);
        public Color pathFindTint = new Color(.3f, .3f, .3f, .3f);
        public Color selectColor = new Color(.4f, .9f, .4f);
        public Color moveColor = new Color(.4f, .4f, .9f);
        public Color attackColor = new Color(.9f, .4f, .4f);

        SpriteRenderer spriteRenderer = null;
        private Color invisible = new Color(0f, 0f, 0f, 0f);
        private Color tint;

        void Start()
        {
            if (tile != null)
            {
                spriteRenderer = tile.gameObject.GetComponentInChildren<SpriteRenderer>();
            }
        }

        void Update()
        {
            if (tile != null)
            {
                if (tile.position.x == 1 && tile.position.y == 1)
                {
                    spriteRenderer.material.SetColor("_RimColor", Color.red);
                }
                if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.Highlighted))
                {
                    tint = hoverTint;
                    spriteRenderer.material.SetColor("_HighlightColor", Color.white - tint);
                }
                else if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.PathFind))
                {
                    tint = pathFindTint;
                    spriteRenderer.material.SetColor("_HighlightColor", Color.white - tint);
                }
                else
                {
                    tint = invisible;
                }

                if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.Attack))
                {
                    spriteRenderer.material.SetColor("_HighlightColor", attackColor - tint);
                }
                if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.Selected))
                {
                    spriteRenderer.material.SetColor("_HighlightColor", selectColor - tint);
                }

                if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.Movable))
                {
                    spriteRenderer.material.SetColor("_HighlightColor", moveColor - tint);
                }

                if (tile.highlightStatus == TileHighlightStatus.None)
                {
                    spriteRenderer.material.SetColor("_HighlightColor", Color.white);
                }
            }
        }
    }
}