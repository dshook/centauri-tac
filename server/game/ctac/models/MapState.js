import _ from 'lodash';
import Position from './Position.js';
import Tile from './Tile.js';
import MapModel from './MapModel.js';
import loglevel from 'loglevel-decorator';
import Constants from '../util/Constants.js';

/**
 * Current loaded map with tile positions
 */
 @loglevel
export default class MapState
{
  constructor()
  {
    this.maps = {};
    this.currentMap = null;
  }

  add(map, setCurrent){
    if(this.maps[map.name]){
      this.log.error('Map %s already registered', map.name);
      return;
    }
    let storedMap = this.maps[map.name] = new MapModel(map.name, map.maxPlayers);

    storedMap.startingPositions = map.startingPositions.map(p => new Position(p.x, p.y, p.z));

    //first get rid of any cosmetic tiles from the tile import list.  These are found just by looking for
    //tiles that are the same x,z position but a lower y (highest tile wins)
    let finalTiles = [];
    for(let tile of map.tiles){
      let sharedPos = map.tiles.filter(sub =>
        sub.transform.x === tile.transform.x &&
        sub.transform.z === tile.transform.z
      );
      if(!sharedPos.length){
        finalTiles.push(tile);
      }else{
        let highestTile = _.maxBy(sharedPos, t => t.transform.y);
        if(highestTile === tile){
          finalTiles.push(highestTile);
        }
      }
    }

    for(let tile of finalTiles){
      let tileModel = new Tile(
        new Position(tile.transform.x, tile.transform.y, tile.transform.z),
        tile.unpassable
      );
      storedMap.tiles.push(tileModel);

      if(!storedMap.tileMatrix[tileModel.position.x]){ storedMap.tileMatrix[tileModel.position.x] = {}; }
      storedMap.tileMatrix[tileModel.position.x][tileModel.position.z] = tileModel;
    }
    this.log.info('Map %s registered with %s/%s tiles remaining', map.name, finalTiles.length, map.tiles.length)

    if(setCurrent){
      this.setMap(map.name);
    }
  }

  setMap(mapName){
    this.currentMap = mapName;
  }

  get map(){
    return this.maps[this.currentMap];
  }

  getTile(position){
    if(!this.map.tileMatrix[position.x]) return null;
    return this.map.tileMatrix[position.x][position.z] || null;
  }

  tileDistance(posA, posB){
    return Math.abs(posA.x - posB.x) + Math.abs(posA.z - posB.z);
  }

  isHeightPassable(startTile, endTile){
    return Math.abs(startTile.position.y - endTile.position.y) < Constants.heightDeltaThreshold;
  }

  kingDistance(posA, posB){
    return Math.max(
      Math.abs(posA.x - posB.x),
      Math.abs(posA.z - posB.z)
    );
  }

  getCrossTiles(center, distance){
    let ret = [];
    ret.push(this.getTile(center).position);

    let neighborTile = null;
    for(let d = 1; d <= distance; d++){
      let toCheck = [
        center.addX(d),
        center.addX(-d),
        center.addZ(d),
        center.addZ(-d)
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
    }
    return ret;
  }

  //uses second point to determine direction of the line, should be within 1 distance of center
  getLineTiles(center, secondPoint, distance, bothDirections){
    let xDiff = secondPoint.x - center.x;
    let zDiff = secondPoint.z - center.z;

    let ret = [];
    ret.push(this.getTile(center).position);
    let neighborTile = null;
    for(let d = 1; d <= distance; d++){
      let toCheck = [
        center.addXYZ(xDiff * d, 0, zDiff * d)
      ];
      if(bothDirections){
        toCheck.push(center.addXYZ(-xDiff * d, 0, -zDiff * d));
      }

      for(let currentDirection of toCheck)
      {
        //check it's not off the map
        neighborTile = this.getTile(currentDirection);
        if (neighborTile != null)
        {
          ret.push(neighborTile.position);
        }
      }
    }
    return ret;
  }

  getKingTilesInRadius(center, distance){
    return this.getTilesInRadiusGeneric(center, distance, this.getKingNeighbors.bind(this), this.kingDistance);
  }

  getTilesInRadius(center, distance){
    return this.getTilesInRadiusGeneric(center, distance, this.getNeighbors.bind(this), this.tileDistance);
  }

  //returns [] of positions for circle around center
  getTilesInRadiusGeneric(center, distance, neighborsFunc, distanceFunc)
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

  getNeighbors(center)
  {
    let ret = [];
    let neighborTile = null;
    let toCheck = [
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

  getKingNeighbors(center)
  {
    let ret = [];
    let neighborTile = null;
    let toCheck = [
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
