using strange.extensions.signal.impl;
using UnityEngine;

namespace ctac.signals
{
    [Singleton]
    public class MinionMoveSignal : Signal<IMinionModel, Tile> { }
}
