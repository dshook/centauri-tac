using UnityEngine;
using System.Collections.Generic;

namespace ctac
{
    public interface IResourceLoaderService
    {
        RuntimeAnimatorController LoadPieceRAC(int pieceId);
        T Load<T>(string resource) where T : class;
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

        private Dictionary<string, object> GOCache = new Dictionary<string, object>();
        public T Load<T>(string resource) where T : class
        {
            if (GOCache.ContainsKey(resource))
            {
                return (T)GOCache[resource];
            }
            var go = Resources.Load(resource) as T;
            GOCache[resource] = go;

            return go;
        }
    }
}

