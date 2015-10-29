using System.Collections.Generic;

namespace ctac
{
    public interface IAnimate
    {
        void Update();
        bool Complete { get; }
        bool Async { get; }
        float? postDelay { get; }
    }

    [Singleton]
    public class AnimationQueueModel
    {
        private List<IAnimate> animations = new List<IAnimate>();
        private List<IAnimate> runningAnimations = new List<IAnimate>();

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
            if (animations.Count > 0)
            {
                IAnimate anim;
                do
                {
                    anim = animations[0];
                    if (anim.Async)
                    {
                        animations.RemoveAt(0);
                        runningAnimations.Add(anim);
                    }
                } while(anim.Async && animations.Count > 0);

                if (!anim.Async)
                {
                    anim.Update();
                    if (anim.Complete)
                    {
                        if(anim.postDelay.HasValue) delayAccumulator = anim.postDelay.Value;
                        animations.RemoveAt(0);
                    }
                }
            }

            foreach (var asyncAnim in runningAnimations)
            {
                asyncAnim.Update();
            }
            runningAnimations.RemoveAll(x => x.Complete);
        }
    }
}
