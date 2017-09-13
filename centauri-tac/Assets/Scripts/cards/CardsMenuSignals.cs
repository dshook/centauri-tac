using strange.extensions.signal.impl;
using System.Collections.Generic;

namespace ctac.signals
{
    [Singleton] public class CardsKickoffSignal : Signal { }

    [Singleton] public class AddCardToDeckSignal : Signal<CardModel> { }
    [Singleton] public class RemoveCardFromDeckSignal : Signal<CardModel> { }

    [Singleton] public class MiniCardHoveredSignal : Signal<CardModel> { }

    [Singleton] public class NewDeckSignal : Signal<DeckModel> { }
    [Singleton] public class EditDeckSignal : Signal<DeckModel> { }
    [Singleton] public class CancelDeckSignal : Signal { }
    [Singleton] public class DeleteDeckSignal : Signal<DeckModel> { }
    [Singleton] public class DeckDeletedSignal : Signal<DeckModel> { }

    [Singleton] public class GetDecksSignal : Signal { }
    [Singleton] public class GotDecksSignal : Signal<ServerDecksModel, SocketKey> { }

    //initial deck saving signal that allows things to prepare and write state
    [Singleton] public class SavingDeckSignal : Signal<DeckModel> { } 
    //the actual save signal triggering the server command
    [Singleton] public class SaveDeckSignal : Signal<DeckModel> { }
    [Singleton] public class DeckSavedSignal : Signal<DeckModel, SocketKey> { }
    [Singleton] public class DeckSaveFailedSignal : Signal<string, SocketKey> { }

    [Singleton] public class CardsMenuMessageSignal : Signal<string> { }

}

