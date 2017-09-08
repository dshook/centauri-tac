using strange.extensions.signal.impl;
using System.Collections.Generic;

namespace ctac.signals
{
    [Singleton] public class CardsKickoffSignal : Signal { }

    [Singleton] public class AddCardToDeckSignal : Signal<CardModel> { }
    [Singleton] public class RemoveCardFromDeckSignal : Signal<CardModel> { }

    [Singleton] public class MiniCardHoveredSignal : Signal<CardModel> { }

    [Singleton] public class NewDeckSignal : Signal<DeckModel> { }
    [Singleton] public class RemoveDeckSignal : Signal<DeckModel> { }
    [Singleton] public class EditDeckSignal : Signal<DeckModel> { }
    [Singleton] public class SaveDeckSignal : Signal<DeckModel> { }
    [Singleton] public class CancelDeckSignal : Signal { }

    [Singleton] public class GetDecksSignal : Signal { }
    [Singleton] public class GotDecksSignal : Signal<ServerDecksModel, SocketKey> { }

}

