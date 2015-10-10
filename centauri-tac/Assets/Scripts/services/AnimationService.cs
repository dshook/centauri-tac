using strange.extensions.mediation.impl;

namespace ctac
{
    public class AnimationService : View
    {
        [Inject]
        public AnimationQueueModel animationQueue { get; set; }

        void Update()
        {
            animationQueue.Update();
        }
    }
}
