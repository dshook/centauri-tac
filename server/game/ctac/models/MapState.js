import _ from 'lodash';
import Position from './Position.js';
import Tile from './Tile.js';
import loglevel from 'loglevel-decorator';

/**
 * Current loaded map with tile positions
 */
 @loglevel
export default class MapState
{
  constructor()
  {
    this.name = '';
    this.maxPlayers = 0;
    this.tiles = [];
  }

  //only really works with one map right now obvs
  add(map){
    this.name = map.name;
    this.maxPlayers = map.maxPlayers;

    for(let tile of map.tiles){
      this.tiles.push(
        new Tile(
          new Position(tile.transform.x, tile.transform.y, tile.transform.z)
        )
      );
    }
  }

  tileDistance(posA, posB){
    return Math.abs(posA.x - posB.x) + Math.abs(posA.y - posB.y);
  }

  //TODO: seems to be bugged and returning smaller distances than actual
  kingDistance(posA, posB){
    return Math.max(
      Math.abs(posA.x - posB.x), 
      Math.abs(posA.z - posB.z)
    );
  }

}
