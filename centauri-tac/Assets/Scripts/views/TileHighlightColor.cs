﻿using UnityEngine;

namespace ctac {
    public class TileHighlightColor : MonoBehaviour
    {
        public Tile tile;
        public Color hoverTint = new Color(.1f, .1f, .1f, .1f);
        public Color selectColor = new Color(.4f, .9f, .4f);
        public Color moveColor = new Color(.4f, .4f, .9f);

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
                if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.Highlighted))
                {
                    tint = hoverTint;
                    spriteRenderer.color = Color.white - tint;
                }
                else
                {
                    tint = invisible;
                }

                if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.Selected))
                {
                    spriteRenderer.color = selectColor - tint;
                }

                if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.Movable))
                {
                    spriteRenderer.color = moveColor - tint;
                }

                if (tile.highlightStatus == TileHighlightStatus.None)
                {
                    spriteRenderer.color = Color.white;
                }
            }
        }
    }
}