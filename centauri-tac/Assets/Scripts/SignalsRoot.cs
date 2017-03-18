using strange.extensions.context.impl;

namespace ctac
{
    public class SignalsRoot : ContextView
    {
        public string startSignalName;

        void Awake()
        {
            context = new SignalsContext(this);
        }
    }
}

