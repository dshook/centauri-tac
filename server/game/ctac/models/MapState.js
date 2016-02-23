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

  getTile(position){
    return this.tiles.find(t => t.position.tileEquals(position));
  }

  tileDistance(posA, posB){
    return Math.abs(posA.x - posB.x) + Math.abs(posA.z - posB.z);
  }

  kingDistance(posA, posB){
    return Math.max(
      Math.abs(posA.x - posB.x),
      Math.abs(posA.z - posB.z)
    );
  }

  getKingTilesInRadius(center: Position, distance: number){
    return this.getTilesInRadiusGeneric(center, distance, this.getKingNeighbors.bind(this), this.kingDistance);
  }

  getTilesInRadius(center: Position, distance: number){
    return this.getTilesInRadiusGeneric(center, distance, this.getNeighbors.bind(this), this.tileDistance);
  }

  //returns [] of positions for circle around center
  getTilesInRadiusGeneric(center: Position, distance: number, neighborsFunc, distanceFunc)
  {
    var ret = [];
    var frontier = [];

    var realCenter = this.getTile(center);
    if(!realCenter) return [];

    frontier.push(realCenter.position);

    while (frontier.length > 0)
    {
      //pop the first item off
      var current = frontier.splice(0, 1)[0];

      if (!ret.find(r => r === current))
      {
        ret.push(current);
      }

      var neighbors = neighborsFunc(current);
      for (let neighbor of neighbors)
      {
        //add the neighbor to explore if it's not already being returned
        //or in the queue or too far away
        if (
          !ret.find(r => r === neighbor)
          && !frontier.find(r => r === neighbor)
          && distanceFunc(neighbor, center) <= distance
        )
        {
          frontier.push(neighbor);
        }
      }
    }

    return ret;
  }

  getNeighbors(center: Position)
  {
    var ret = [];
    var neighborTile = null;
    var toCheck = [
        center.addX(1),
        center.addX(-1),
        center.addZ(1),
        center.addZ(-1)
    ];

    for(let currentDirection of toCheck)
    {
      //check it's not off the map
      neighborTile = this.getTile(currentDirection);
      if (neighborTile != null)
      {
        ret.push(neighborTile.position);
      }
    }
    return ret;
  }

  getKingNeighbors(center: Position)
  {
    var ret = [];
    var neighborTile = null;
    var toCheck = [
        center.addX(1),
        center.addX(-1),
        center.addZ(1),
        center.addZ(-1),

        center.addXYZ(-1, 0, -1),
        center.addXYZ(-1, 0, 1),
        center.addXYZ(1, 0, -1),
        center.addXYZ(1, 0, 1),
    ];

    for(let currentDirection of toCheck)
    {
      //check it's not off the map
      neighborTile = this.getTile(currentDirection);
      if (neighborTile != null)
      {
        ret.push(neighborTile.position);
      }
    }
    return ret;
  }

}
