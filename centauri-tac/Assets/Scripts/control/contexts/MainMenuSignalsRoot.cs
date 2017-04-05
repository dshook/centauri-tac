using strange.extensions.context.impl;

namespace ctac
{
    public class MainMenuSignalsRoot : ContextView
    {
        void Awake()
        {
            context = new MainMenuSignalsContext(this);
        }
    }
}

