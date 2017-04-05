using strange.extensions.context.impl;

namespace ctac
{
    public class GameSignalsRoot : ContextView
    {
        public string startSignalName;

        void Awake()
        {
            context = new GameSignalsContext(this);
        }
    }
}

