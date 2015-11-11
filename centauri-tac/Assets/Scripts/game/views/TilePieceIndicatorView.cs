using UnityEngine;
using strange.extensions.mediation.impl;
using System.Collections.Generic;

namespace ctac
{
    public class TilePieceIndicatorView : View
    {
        private Color enemyColor = new Color(0.5f, .1f, .1f);
        private Color friendlyColor = new Color(0.1f, .35f, .1f);

        private float borderWidth = 1.32f;

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
                ClearTile(tile);
            }
        }

        internal void ClearTile(Tile tile)
        {
            var renderer = tile.gameObject.GetComponentInChildren<MeshRenderer>();

            renderer.material.SetFloat("_BorderWidth", 0f);
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
            var renderer = tile.gameObject.GetComponentInChildren<MeshRenderer>();

            renderer.material.SetColor("_RimColor", color);
            renderer.material.SetFloat("_BorderWidth", borderWidth);
        }

    }
}

