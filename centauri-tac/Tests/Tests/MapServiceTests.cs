using Microsoft.VisualStudio.TestTools.UnitTesting;
using ctac;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class MapServiceTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var mapService = new MapService();
            mapService.mapModel = new MapModel()
            {
                name = "test map",
                maxPlayers = 0,
                root = null,
                tiles = CreateTiles(10, 10)
            };
            
            var tilesInRadius = mapService.GetTilesInRadius(new Vector2(2, 2), 2);
            var expectedTiles = new List<Vector2>()
            {
                new Vector2(2, 2),
                new Vector2(1, 2),
                new Vector2(0, 2),
                new Vector2(3, 2),
                new Vector2(4, 2),
                new Vector2(2, 3),
                new Vector2(2, 4),
                new Vector2(2, 1),
                new Vector2(2, 0),
                new Vector2(1, 1),
                new Vector2(3, 3),
                new Vector2(1, 3),
                new Vector2(3, 1),
            };

            CollectionAssert.AreEqual(
                tilesInRadius.Select(x => x.Key).OrderBy(v => v.x).ThenBy(v => v.y).ToList(), 
                expectedTiles.OrderBy(v => v.x).ThenBy(v => v.y).ToList());
        }

        private Dictionary<Vector2, Tile> CreateTiles(int x, int z)
        {
            var tiles = new Dictionary<Vector2, Tile>();
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < z; j++)
                {
                    var fullPosition = new Vector3(i, 0, j);
                    var position = new Vector2(i, j);
                    var newTile = new Tile() {
                        gameObject = null,
                        position = position,
                        fullPosition = fullPosition
                    };
                    tiles.Add(position, newTile );
                }
            }
            return tiles;
        }
    }
}
