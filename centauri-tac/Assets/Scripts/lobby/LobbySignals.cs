using strange.extensions.signal.impl;

namespace ctac.signals
{
    [Singleton] public class AuthLobbySignal : Signal<PlayerModel, SocketKey> { }
    [Singleton] public class LobbyLoggedInSignal : Signal<LoginStatusModel, SocketKey> { }
    [Singleton] public class SwitchLobbyViewSignal : Signal<LobbyScreens> { }
}
