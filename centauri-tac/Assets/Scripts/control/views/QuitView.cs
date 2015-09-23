using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using UnityEngine;

namespace ctac
{
    public class QuitView : View
    {
        public Signal quit = new Signal();

        void OnApplicationQuit()
        {
            quit.Dispatch();
        }

    }
}
