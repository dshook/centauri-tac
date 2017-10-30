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

        [Inject] public IResourceLoaderService loader { get; set; }

        GameObject mapTilePrefab;
        Dictionary<string, Material> mapMaterials = new Dictionary<string, Material>();

        public void CreateMap(MapImportModel map)
        {
            mapTilePrefab = loader.Load<GameObject>("Maps/Tiles/Tile");

            mapMaterials["clay"] = loader.Load<Material>("Maps/Tiles/tile_clay");
            mapMaterials["grass"] = loader.Load<Material>("Maps/Tiles/tile_grass");
            mapMaterials["rock"] = loader.Load<Material>("Maps/Tiles/tile_rock");
            mapMaterials["sand"] = loader.Load<Material>("Maps/Tiles/tile_sand");
            mapMaterials["water"] = loader.Load<Material>("Maps/Tiles/tile_water");
            mapMaterials["invisible"] = loader.Load<Material>("Maps/Tiles/tile_invisible");

            var goMap = GameObject.Find("Map");
            if (goMap != null)
            {
                GameObject.DestroyImmediate(goMap);
            }
            goMap = new GameObject("Map");
            goMap.transform.parent = contextView.transform;

            var tilesGO = new GameObject("Tiles");
            tilesGO.transform.parent = goMap.transform;

            var propsGO = new GameObject("Props");
            propsGO.transform.parent = goMap.transform;

            mapModel.root = goMap;
            mapModel.name = map.name;
            mapModel.maxPlayers = map.maxPlayers;
            mapModel.tiles = new Dictionary<Vector2, Tile>();
            mapModel.cosmeticTiles = new List<Tile>();
            mapModel.props = new List<PropView>();

            goMap.AddComponent<MapView>();

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

            setupTiles(map.tiles, tilesGO, false, map.startingPositions);
            setupTiles(dedupedCosmeticTiles, goMap, true, null);

            setupProps(propsGO, map.props);

            mapCreated.Dispatch();
        }

        void setupTiles(List<TileImport> tiles, GameObject parentGO, bool areCosmetic, List<TileImportPosition> startingPositions)
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
                newTileGO.transform.parent = parentGO.transform;

                //set up material
                var tileRenderer = newTileGO.transform.Find("cube").GetComponent<MeshRenderer>();
                if (!mapMaterials.ContainsKey(t.material))
                {
                    Debug.LogError("Material " + t.material + " not loaded");
                    return;
                }
                tileRenderer.material = mapMaterials[t.material];
                //var tileVarietyColor = Random.Range(0.85f, 1);
                //tileRenderer.material.color = new Color(tileVarietyColor, tileVarietyColor, tileVarietyColor);

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

                var tileView = newTileGO.GetComponent<TileView>();
                tileView.tile = newTile;
                tileView.unpassable = newTile.unpassable;
                tileView.Init();

                if (!areCosmetic)
                {
                    var indicator = newTileGO.transform.Find("Indicator").gameObject;
                    indicator.AddComponent<TilePieceIndicatorialView>();
                }

                if (startingPositions != null && startingPositions.Any(ti => ti.x == fullPosition.x && ti.z == fullPosition.z))
                {
                    tileView.isStartTile = true;
                }
            }
        }

        private void setupProps(GameObject propsRoot, List<PropImport> props)
        {
            if (props == null || propsRoot == null) return; 
            foreach (var prop in props)
            {
                var prefab = loader.Load<GameObject>("Maps/Props/" + prop.propName);

                var newProp = GameObject.Instantiate(
                    prefab, 
                    new Vector3(prop.transform.x, prop.transform.y, prop.transform.z), 
                    Quaternion.Euler(prop.rotation.x, prop.rotation.y, prop.rotation.z)
                ) as GameObject;
                newProp.transform.parent = propsRoot.transform;

                var propView = newProp.GetComponent<PropView>();
                propView.breakable = prop.breakable;

                mapModel.props.Add(propView);
            }
        }

    }
}

