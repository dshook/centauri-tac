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

        public void CreateMap(MapImportModel map)
        {
            var mapTilePrefab = Resources.Load("Tile") as GameObject;

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
                var newTileGO = GameObject.Instantiate(
                    mapTilePrefab, 
                    new Vector3(t.transform.x, t.transform.y, t.transform.z), 
                    Quaternion.identity
                ) as GameObject;
                newTileGO.transform.parent = goMap.transform;

                var position = new Vector2(t.transform.x, t.transform.z);
                var newTile = new Tile() {
                        gameObject = newTileGO,
                        position = position
                    };
                mapModel.tiles.Add(position, newTile );

                newTileGO.AddComponent<TileHighlightColor>();
                newTileGO.GetComponent<TileHighlightColor>().tile = newTile;
            }

            mapCreated.Dispatch();
        }

    }
}

