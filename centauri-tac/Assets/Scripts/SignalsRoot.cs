using strange.extensions.context.impl;

namespace ctac
{
    public class SignalsRoot : ContextView
    {

        void Awake()
        {
            context = new SignalsContext(this);
        }
    }
}

