using strange.extensions.signal.impl;

namespace ctac.signals
{
    [Singleton] public class CardsKickoffSignal : Signal { }

    [Singleton] public class AddCardToDeckSignal : Signal<CardModel> { }
    [Singleton] public class RemoveCardFromDeckSignal : Signal<CardModel> { }

    [Singleton] public class MiniCardHoveredSignal : Signal<CardModel> { }

}

