﻿using UnityEngine;

namespace ctac {
    public class TileView : MonoBehaviour
    {
        public Tile tile;
        public Color hoverTint = new Color(.1f, .1f, .1f, .1f);
        public Color pathFindTint = new Color(.3f, .3f, .3f, .3f);
        public Color selectColor = new Color(.4f, .9f, .4f);
        public Color moveColor = new Color(.4f, .4f, .9f);
        public Color moveRangeColor = new Color(.6f, .6f, 1f);
        public Color attackColor = new Color(.9f, .4f, .4f);
        public Color attackRangeColor = new Color(1f, .6f, .6f);

        private MeshRenderer meshRenderer = null;
        private GameObject arrows = null;
        private Color invisible = new Color(0f, 0f, 0f, 0f);
        private Color tint;

        void Start()
        {
            if (tile != null)
            {
                meshRenderer = tile.gameObject.GetComponentInChildren<MeshRenderer>();
                arrows = tile.gameObject.transform.FindChild("Arrows").gameObject;
            }
        }

        void Update()
        {
            if (tile == null) return;

            if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.Highlighted))
            {
                tint = hoverTint;
                meshRenderer.material.SetColor("_HighlightColor", Color.white - tint);
            }
            else if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.PathFind))
            {
                tint = pathFindTint;
                meshRenderer.material.SetColor("_HighlightColor", Color.white - tint);
            }
            else
            {
                tint = invisible;
            }

            if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.Attack))
            {
                meshRenderer.material.SetColor("_HighlightColor", attackColor - tint);
            }
            if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.Selected))
            {
                meshRenderer.material.SetColor("_HighlightColor", selectColor - tint);
            }
            if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.Movable))
            {
                meshRenderer.material.SetColor("_HighlightColor", moveColor - tint);
            }
            if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.MoveRange))
            {
                meshRenderer.material.SetColor("_HighlightColor", moveRangeColor - tint);
            }
            if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.AttackRange))
            {
                meshRenderer.material.SetColor("_HighlightColor", attackRangeColor - tint);
            }

            if (tile.highlightStatus == TileHighlightStatus.None)
            {
                meshRenderer.material.SetColor("_HighlightColor", Color.white);
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