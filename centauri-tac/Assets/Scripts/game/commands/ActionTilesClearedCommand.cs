using strange.extensions.command.impl;
using ctac.signals;

namespace ctac
{
    public class ActionTilesClearedCommand : Command
    {
        [Inject] public SocketKey socketKey { get; set; }

        [Inject] public TilesClearedModel tilesCleared { get; set; }
        [Inject] public TilesClearedSignal tilesClearedSignal { get; set; }

        [Inject] public IDebugService debug { get; set; }
        [Inject] public MapModel map { get; set; }
        [Inject] public ActionsProcessedModel processedActions { get; set; }
        
        public override void Execute()
        {
            if (!processedActions.Verify(tilesCleared.id)) return;

            foreach (var tilePosition in tilesCleared.tilePositions)
            {
                if (map.tiles.ContainsKey(tilePosition.Vector2))
                {
                    var tile = map.tiles[tilePosition.Vector2];
                    tile.unpassable = false;
                }
            }

            tilesClearedSignal.Dispatch(tilesCleared);
            debug.Log(string.Format("{0} Tiles cleared", tilesCleared.tilePositions.Count), socketKey);
        }
    }
}

