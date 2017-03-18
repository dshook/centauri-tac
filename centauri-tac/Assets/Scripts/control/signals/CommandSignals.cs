using strange.extensions.signal.impl;

namespace ctac.signals
{
    [ManualMapSignal]
	public class StartSignal : Signal { }

    [ManualMapSignal]
	public class PiecesStartSignal : Signal { }

    [Singleton]
    public class QuitSignal : Signal { }
}

