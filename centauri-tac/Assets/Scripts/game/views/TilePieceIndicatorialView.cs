using UnityEngine;
using strange.extensions.mediation.impl;
using SVGImporter;
using ctac.util;

namespace ctac
{
    public class TilePieceIndicatorialView : View
    {
        private Color enemyColor = ColorExtensions.HexToColor("FF0000");
        private Color friendlyColor = ColorExtensions.HexToColor("00FF00");

        private new SVGRenderer svgRenderer;
        private bool active = false;

        private Vector3 minSize = new Vector3(0.85f, 0.85f, 1);
        private Vector3 maxSize = Vector3.one;

        protected override void Start()
        {
            svgRenderer = GetComponent<SVGRenderer>();
        }

        void Update()
        {
            if(!active) return;

            //rotate and bounce scale
            transform.Rotate(Vector3.forward, 2f);
            var scaleFactor = Mathf.Sin(Time.time / 0.33f) * 0.5f + 0.5f;
            transform.localScale = Vector3.Lerp(minSize, maxSize, scaleFactor);
        }

        public void SetStatus(bool newStatus, bool? isEnemy = null)
        {
            active = newStatus;
            svgRenderer.enabled = active;
            if (isEnemy.HasValue)
            {
                svgRenderer.color = isEnemy.Value ? enemyColor : friendlyColor;
            }
        }

        internal void SetColor(Color color)
        {
            svgRenderer.color = color;
        }

    }
}

