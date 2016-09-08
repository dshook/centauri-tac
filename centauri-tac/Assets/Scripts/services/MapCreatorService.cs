using UnityEngine;
using strange.extensions.context.api;
using System.Collections.Generic;
using ctac.signals;

namespace ctac
{
    public interface IMapCreatorService
    {
        void CreateMap(MapImportModel map);
    }

    public class MapCreatorService : IMapCreatorService
    {
        [Inject(ContextKeys.CONTEXT_VIEW)]
        public GameObject contextView { get; set; }

        [Inject]
        public MapCreatedSignal mapCreated { get; set; }

        [Inject]
        public MapModel mapModel { get; set; }

        [Inject]
        public PiecesModel pieces { get; set; }

        public void CreateMap(MapImportModel map)
        {
            var mapTilePrefab = Resources.Load("Tile") as GameObject;

            var mapMaterials = new Dictionary<string, Material>();
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

            foreach (var t in map.tiles)
            {
                var fullPosition = new Vector3(t.transform.x, t.transform.y, t.transform.z);
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
                var position = new Vector2(t.transform.x, t.transform.z);
                var newTile = new Tile() {
                        gameObject = newTileGO,
                        position = position,
                        fullPosition = fullPosition,
                        unpassable = t.unpassable
                    };
                mapModel.tiles.Add(position, newTile );

                newTileGO.AddComponent<TileView>();
                newTileGO.GetComponent<TileView>().tile = newTile;

                var indicator = newTileGO.transform.Find("Indicator").gameObject;
                var indicatorView = indicator.AddComponent<TilePieceIndicatorialView>();
                indicatorView.pieces = pieces;
            }

            mapCreated.Dispatch();
        }

    }
}

