using strange.extensions.signal.impl;

namespace ctac.signals
{

    [Singleton]
    public class TileHoverSignal : Signal<Tile> { }

    [Singleton]
    public class MapCreatedSignal : Signal { }

    [Singleton]
    public class TurnEndedSignal : Signal { }

    [Singleton]
    public class MinionSelectedSignal : Signal<MinionModel> { }

    [Singleton]
    public class MinionMoveSignal : Signal<MinionModel, Tile> { }

    [Singleton]
    public class EndTurnSignal : Signal { }
}

