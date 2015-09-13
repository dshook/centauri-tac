using UnityEngine;
using strange.extensions.context.api;
using System.Collections.Generic;
using ctac.signals;

namespace ctac
{
    public interface IMapCreatorService
    {
        void CreateMap(IMapImportModel map);
    }

    public class MapCreatorService : IMapCreatorService
    {
        [Inject(ContextKeys.CONTEXT_VIEW)]
        public GameObject contextView { get; set; }

        [Inject]
        public MapCreatedSignal mapCreated { get; set; }

        [Inject]
        public IMapModel mapModel { get; set; }

        public void CreateMap(IMapImportModel map)
        {
            var mapTilePrefab = Resources.Load("Tile") as GameObject;

            var goMap = GameObject.Find("Map");
            if (goMap != null)
            {
                GameObject.Destroy(goMap);
            }
            goMap = new GameObject("Map");
            goMap.transform.parent = contextView.transform;
            goMap.AddComponent<TileHighlightView>();

            mapModel.root = goMap;
            mapModel.name = map.name;
            mapModel.maxPlayers = map.maxPlayers;
            mapModel.tiles = new List<Tile>();

            foreach (var t in map.tiles)
            {
                var newTile = GameObject.Instantiate(
                    mapTilePrefab, 
                    new Vector3(t.transform.x, t.transform.y, t.transform.z), 
                    Quaternion.identity
                ) as GameObject;
                newTile.transform.parent = goMap.transform;

                mapModel.tiles.Add(new Tile() { gameObject = newTile });
            }

            mapCreated.Dispatch();
        }

    }
}

