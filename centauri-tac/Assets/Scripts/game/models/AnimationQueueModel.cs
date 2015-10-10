using System.Collections.Generic;
using System.Linq;

namespace ctac
{
    public interface IAnimate
    {
        void Update();
        bool Complete { get; }
    }

    [Singleton]
    public class AnimationQueueModel
    {
        private List<IAnimate> animations = new List<IAnimate>();

        public void Add(IAnimate animation)
        {
            animations.Add(animation);
        }

        public void Update()
        {
            if (animations.Count > 0)
            {
                var anim = animations[0];
                anim.Update();
                if (anim.Complete)
                {
                    animations.RemoveAt(0);
                }
            }
        }
    }
}
