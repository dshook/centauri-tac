/// A Signal which hands back an URL
/// 
/// string The URL
using strange.extensions.signal.impl;

namespace ctac
{
    [Singleton]
    public class SocketMessageSignal : Signal<string> { }
}
