using strange.extensions.signal.impl;

namespace ctac.signals
{
    [ManualMapSignal]
	public class StartSignal : Signal { }

    [ManualMapSignal]
	public class PiecesStartSignal : Signal { }

    [ManualMapSignal]
	public class MainMenuStartSignal : Signal { }

    [ManualMapSignal]
	public class CardsMenuStartSignal : Signal { }

    [Singleton]
    public class QuitSignal : Signal { }
}

