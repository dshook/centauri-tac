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

        new MeshRenderer renderer = null;
        private Color invisible = new Color(0f, 0f, 0f, 0f);
        private Color tint;

        void Start()
        {
            if (tile != null)
            {
                renderer = tile.gameObject.GetComponentInChildren<MeshRenderer>();
            }
        }

        void Update()
        {
            if (tile != null)
            {
                if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.Highlighted))
                {
                    tint = hoverTint;
                    renderer.material.SetColor("_HighlightColor", Color.white - tint);
                }
                else if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.PathFind))
                {
                    tint = pathFindTint;
                    renderer.material.SetColor("_HighlightColor", Color.white - tint);
                }
                else
                {
                    tint = invisible;
                }

                if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.Attack))
                {
                    renderer.material.SetColor("_HighlightColor", attackColor - tint);
                }
                if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.Selected))
                {
                    renderer.material.SetColor("_HighlightColor", selectColor - tint);
                }

                if (FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.Movable))
                {
                    renderer.material.SetColor("_HighlightColor", moveColor - tint);
                }

                if (tile.highlightStatus == TileHighlightStatus.None)
                {
                    renderer.material.SetColor("_HighlightColor", Color.white);
                }
            }
        }
    }
}