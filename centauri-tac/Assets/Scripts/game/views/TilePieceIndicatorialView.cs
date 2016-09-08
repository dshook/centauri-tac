using UnityEngine;
using strange.extensions.mediation.impl;
using SVGImporter;

namespace ctac
{
    public class TilePieceIndicatorialView : View
    {
        public PiecesModel pieces { get; set; }

        //private Color enemyColor = ColorExtensions.HexToColor("FF0000");
        //private Color friendlyColor = ColorExtensions.HexToColor("00FF00");

        private new SVGRenderer renderer;
        private TileView tile;
        private bool active = false;

        private Vector3 minSize = new Vector3(0.85f, 0.85f, 1);
        private Vector3 maxSize = Vector3.one;

        protected override void Start()
        {
            renderer = GetComponent<SVGRenderer>();
            tile = GetComponentInParent<TileView>();
        }

        void Update()
        {
            //update status
            if (FlagsHelper.IsSet(tile.tile.highlightStatus, TileHighlightStatus.AttackRange)
                && pieces != null
                && pieces.PieceAt(tile.tile.position) != null)
            {
                if (!active) SetStatus(true);
            }
            else
            {
                SetStatus(false);
            }

            if(!active) return;

            //rotate and bounce scale
            transform.Rotate(Vector3.forward, 2f);
            var scaleFactor = Mathf.Sin(Time.time / 0.33f) * 0.5f + 0.5f;
            transform.localScale = Vector3.Lerp(minSize, maxSize, scaleFactor);
        }

        internal void SetStatus(bool newStatus)
        {
            active = newStatus;

            renderer.enabled = active;
        }

        internal void SetColor(Color color)
        {
            renderer.color = color;
        }

    }
}

