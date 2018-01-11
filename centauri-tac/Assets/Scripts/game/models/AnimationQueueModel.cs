using System.Collections.Generic;

namespace ctac
{
    public interface IAnimate
    {
        void Update();
        void Init();
        bool Complete { get; }
        bool Async { get; }
        float? postDelay { get; }
    }

    [GameSingleton]
    public class AnimationQueueModel
    {
        private List<IAnimate> animations = new List<IAnimate>();
        private List<IAnimate> runningAnimations = new List<IAnimate>();
        private bool stoppedForSync = false;

        private float delayAccumulator = 0f;

        public void Add(IAnimate animation)
        {
            animations.Add(animation);
        }

        public void Update(float deltaTime)
        {
            if (delayAccumulator > 0)
            {
                delayAccumulator -= deltaTime;
                return;
            }

            //queue up animations until we run out or get stopped for a synchronous animation
            if (animations.Count > 0 && !stoppedForSync)
            {
                IAnimate anim;
                do
                {
                    anim = animations[0];
                    animations.RemoveAt(0);
                    anim.Init();
                    runningAnimations.Add(anim);
                    if (!anim.Async)
                    {
                        stoppedForSync = true;
                    }
                } while(anim.Async && animations.Count > 0);
            }

            //process all running animations
            foreach (var anim in runningAnimations)
            {
                anim.Update();
                if (anim.Complete)
                {
                    if(anim.postDelay.HasValue) delayAccumulator = anim.postDelay.Value;
                    if (!anim.Async)
                    {
                        stoppedForSync = false;
                    }
                }
            }
            runningAnimations.RemoveAll(x => x.Complete);
        }
    }
}
