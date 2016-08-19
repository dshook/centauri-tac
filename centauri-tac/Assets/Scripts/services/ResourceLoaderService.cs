using UnityEngine;
using System.Collections.Generic;

namespace ctac
{
    public interface IResourceLoaderService
    {
        RuntimeAnimatorController LoadPieceRAC(int pieceId);
        GameObject Load(string resource);
    }

    public class ResourceLoaderService : IResourceLoaderService
    {
        private Dictionary<int, RuntimeAnimatorController> RacCache = new Dictionary<int, RuntimeAnimatorController>();
        public RuntimeAnimatorController LoadPieceRAC(int pieceId)
        {
            if (RacCache.ContainsKey(pieceId))
            {
                return RacCache[pieceId];
            }

            var animationController = Resources.Load("Pieces/" + pieceId + "/Unit") as RuntimeAnimatorController;
            RacCache[pieceId] = animationController;
            return animationController;
        }

        private Dictionary<string, GameObject> GOCache = new Dictionary<string, GameObject>();
        public GameObject Load(string resource)
        {
            if (GOCache.ContainsKey(resource))
            {
                return GOCache[resource];
            }
            var go = Resources.Load(resource) as GameObject;
            GOCache[resource] = go;

            return go;
        }
    }
}

