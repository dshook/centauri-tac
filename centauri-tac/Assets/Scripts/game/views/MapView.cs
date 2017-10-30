using UnityEngine;
using strange.extensions.mediation.impl;
using System.Collections.Generic;

namespace ctac
{
    public class MapView : View
    {
        public class ClearPropAnim : IAnimate
        {
            public bool Complete { get; set; }
            public bool Async { get { return true; } }
            public float? postDelay { get { return null; } }

            public GameObject particleEffectPrefab { get; set; }
            public Vector3 particleLocation { get; set; }
            public List<GameObject> propsToDestroy { get; set; }

            private GameObject spawnedParticles = null;
            private float runTime = 0f;

            public void Init() {
                spawnedParticles = GameObject.Instantiate(particleEffectPrefab, particleLocation, Quaternion.Euler(-90f, 0, 0));
                foreach (var prop in propsToDestroy)
                {
                    Destroy(prop, 0.8f);
                }
            }
            public void Update()
            {
                runTime += Time.deltaTime;

                if (runTime > 2.5f)
                {
                    Destroy(spawnedParticles);
                    Complete = true;
                }
            }
        }
    }

}

