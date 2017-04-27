using strange.extensions.context.impl;

namespace ctac
{
    public class CardsMenuSignalsRoot : ContextView
    {
        void Awake()
        {
            context = new CardsMenuSignalsContext(this);
        }
    }
}

