using UnityEngine;
using strange.extensions.context.api;
using System.Collections.Generic;
using ctac.signals;
using System.Linq;

namespace ctac
{
    public interface IMapCreatorService
    {
        void CreateMap(MapImportModel map);
    }

    public class MapCreatorService : IMapCreatorService
    {
        [Inject(InjectionKeys.GameSignalsRoot)]
        public GameObject contextView { get; set; }

        [Inject]
        public MapCreatedSignal mapCreated { get; set; }

        [Inject]
        public MapModel mapModel { get; set; }

        GameObject mapTilePrefab;
        Dictionary<string, Material> mapMaterials = new Dictionary<string, Material>();

        public void CreateMap(MapImportModel map)
        {
            mapTilePrefab = Resources.Load("Tile") as GameObject;

            mapMaterials["clay"] = Resources.Load("Materials/tiles/tile_clay") as Material;
            mapMaterials["grass"] = Resources.Load("Materials/tiles/tile_grass") as Material;
            mapMaterials["rock"] = Resources.Load("Materials/tiles/tile_rock") as Material;
            mapMaterials["sand"] = Resources.Load("Materials/tiles/tile_sand") as Material;
            mapMaterials["water"] = Resources.Load("Materials/tiles/tile_water") as Material;

            var goMap = GameObject.Find("Map");
            if (goMap != null)
            {
                GameObject.Destroy(goMap);
            }
            goMap = new GameObject("Map");
            goMap.transform.parent = contextView.transform;

            mapModel.root = goMap;
            mapModel.name = map.name;
            mapModel.maxPlayers = map.maxPlayers;
            mapModel.tiles = new Dictionary<Vector2, Tile>();
            mapModel.cosmeticTiles = new List<Tile>();

            //first go through the import looking for tiles stacked on top of each other.
            //the topmost tile will be the "real" map tile and ones below that will be instanciated as normal
            //but just be cosmetic. Kinda nasty self join stuff going on here
            var cosmeticTiles = from t in map.tiles
                                join t1 in map.tiles on new { t.transform.x, t.transform.z } 
                                    equals new { t1.transform.x, t1.transform.z }
                                where t1.transform.y < t.transform.y
                                select t1;
            //dedupe if there are 3 stacked in the same column
            var dedupedCosmeticTiles = cosmeticTiles
                .GroupBy(t => t.transform, (key, c) => c.FirstOrDefault())
                .ToList();

            //and remove cosmetic from normal tiles
            foreach (var cosmeticTile in dedupedCosmeticTiles)
            {
                map.tiles.Remove(cosmeticTile);
            }

            setupTiles(map.tiles, goMap, false);
            setupTiles(dedupedCosmeticTiles, goMap, true);

            mapCreated.Dispatch();
        }

        void setupTiles(List<TileImport> tiles, GameObject goMap, bool areCosmetic)
        {

            foreach (var t in tiles)
            {
                var fullPosition = new Vector3(t.transform.x, t.transform.y, t.transform.z);
                var position = new Vector2(t.transform.x, t.transform.z);
                if (!areCosmetic && mapModel.tiles.ContainsKey(position))
                {
                    Debug.LogWarning("Map already contains tile at " + position);
                    continue;
                }
                var newTileGO = GameObject.Instantiate(
                    mapTilePrefab, 
                    fullPosition, 
                    Quaternion.identity
                ) as GameObject;
                newTileGO.transform.parent = goMap.transform;

                //set up material
                var tileRenderer = newTileGO.transform.Find("cube").GetComponent<MeshRenderer>();
                tileRenderer.material = mapMaterials[t.material];
                var tileVarietyColor = Random.Range(0.85f, 1);
                tileRenderer.material.color = new Color(tileVarietyColor, tileVarietyColor, tileVarietyColor);

                //position and set up map references
                var newTile = new Tile() {
                        gameObject = newTileGO,
                        position = position,
                        fullPosition = fullPosition,
                        unpassable = t.unpassable
                    };

                if (areCosmetic)
                {
                    mapModel.cosmeticTiles.Add(newTile);
                }
                else
                {
                    mapModel.tiles.Add(position, newTile);
                }

                newTileGO.AddComponent<TileView>();
                newTileGO.GetComponent<TileView>().tile = newTile;

                if (!areCosmetic)
                {
                    var indicator = newTileGO.transform.Find("Indicator").gameObject;
                    indicator.AddComponent<TilePieceIndicatorialView>();
                }
            }
        }

    }
}

