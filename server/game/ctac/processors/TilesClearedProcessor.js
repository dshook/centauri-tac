import TilesCleared from '../actions/TilesCleared.js';
import loglevel from 'loglevel-decorator';

@loglevel
export default class TilesClearedProcessor
{
  constructor(mapState)
  {
    this.mapState = mapState;
  }

  /**
   * Proc
   */
  async handleAction(action, queue)
  {
    if (!(action instanceof TilesCleared)) {
      return;
    }

    let tiles = action.tilePositions
      .map(p => this.mapState.getTile(p))
      .filter(p => p);

    for(let tile of tiles){
      if(!tile.clearable){
        this.log.warn('Cannot set tile at %j as passable when it is not clearable', tile.position);
        continue;
      }
      tile.unpassable = false;
    }

    queue.complete(action);
  }
}
