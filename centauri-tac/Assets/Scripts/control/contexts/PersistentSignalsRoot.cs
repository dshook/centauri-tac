using strange.extensions.context.impl;

namespace ctac
{
    public class PersistentSignalsRoot : ContextView
    {
        void Awake()
        {
            context = new PersistentSignalsContext(this);
        }
    }
}

