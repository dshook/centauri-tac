using strange.extensions.signal.impl;

namespace ctac.signals
{
    [ManualMapSignal]
	public class StartSignal : Signal { }

    [Singleton]
    public class QuitSignal : Signal { }
}

