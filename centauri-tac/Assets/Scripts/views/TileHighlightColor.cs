using UnityEngine;

namespace ctac {
    public class TileHighlightColor : MonoBehaviour
    {
        public Tile tile;
        public Color hoverTint = new Color(.9f, .9f, .9f, .9f);
        public Color selectColor = new Color(.4f, .9f, .4f);
        public Color moveColor = new Color(.4f, .4f, .9f);

        SpriteRenderer spriteRenderer = null;

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
                if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.Highlighted))
                {
                    spriteRenderer.color = hoverTint;
                }

                if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.Selected))
                {
                    spriteRenderer.color = selectColor;
                }

                if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.Movable))
                {
                    spriteRenderer.color = moveColor;
                }

                if (tile.highlightStatus == TileHighlightStatus.None)
                {
                    spriteRenderer.color = Color.white;
                }
            }
        }
    }
}