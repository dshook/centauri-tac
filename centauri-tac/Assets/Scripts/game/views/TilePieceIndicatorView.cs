using UnityEngine;
using strange.extensions.mediation.impl;
using System.Collections.Generic;

namespace ctac
{
    public class TilePieceIndicatorView : View
    {
        private Color enemyColor = new Color(.8f, .0f, .0f, .6f);
        private Color friendlyColor = new Color(0.0f, 0.8f, .0f, .6f);

        //private float borderWidth = 0.5f;

        internal void init()
        {
        }

        void Update()
        {
        }

        //lil boilerplate for all the combos
        internal void ClearTiles(List<Tile> tiles)
        {
            foreach (var tile in tiles)
            {
                if (tile.indicator == null)
                {
                    tile.indicator = tile.gameObject.transform.FindChild("Indicator").gameObject;
                }
                ClearTile(tile);
            }
        }

        internal void ClearTile(Tile tile)
        {
            tile.indicator.SetActive(false);
        }

        internal void SetFriendly(List<Tile> tiles)
        {
            SetColor(tiles, friendlyColor);
        }
        internal void SetFriendly(Tile tile)
        {
            SetColor(tile, friendlyColor);
        }

        internal void SetEnemy(List<Tile> tiles)
        {
            SetColor(tiles, enemyColor);
        }
        internal void SetEnemy(Tile tile)
        {
            SetColor(tile, enemyColor);
        }

        internal void SetColor(List<Tile> tiles, Color color)
        {
            foreach (var tile in tiles)
            {
                SetColor(tile, color);
            }
        }

        internal void SetColor(Tile tile, Color color)
        {
            var renderer = tile.indicator.GetComponent<MeshRenderer>();

            //renderer.material.SetColor("_RimColor", color);
            renderer.material.color = color;
            tile.indicator.SetActive(true);
        }

    }
}

