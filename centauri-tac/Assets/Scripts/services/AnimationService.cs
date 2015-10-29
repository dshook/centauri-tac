using strange.extensions.mediation.impl;
using UnityEngine;

namespace ctac
{
    public class AnimationService : View
    {
        [Inject]
        public AnimationQueueModel animationQueue { get; set; }

        void Update()
        {
            animationQueue.Update(Time.deltaTime);
        }
    }
}
