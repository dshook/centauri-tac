using UnityEngine;
using strange.extensions.context.api;
using System.Collections.Generic;

namespace ctac
{
    public interface IMapCreatorService
    {
        void CreateMap(IMapModel map);
    }

    public class MapCreatorService : IMapCreatorService
    {
        [Inject(ContextKeys.CONTEXT_VIEW)]
        public GameObject contextView { get; set; }

        public void CreateMap(IMapModel map)
        {
            var mapTilePrefab = Resources.Load("Tile") as GameObject;

            var goMap = GameObject.Find("Map");
            if (goMap != null)
            {
                GameObject.Destroy(goMap);
            }
            goMap = new GameObject("Map");
            goMap.transform.parent = contextView.transform;

            foreach (var t in map.tiles)
            {
                var newTile = GameObject.Instantiate(mapTilePrefab, new Vector3(t.transform.x, t.transform.y, t.transform.z), Quaternion.identity) as GameObject;
                newTile.transform.parent = goMap.transform;
            }

        }

    }
}

