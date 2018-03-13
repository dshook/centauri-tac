import _ from 'lodash';
import Position from './Position.js';
import Tile from './Tile.js';
import MapModel from './MapModel.js';
import Statuses from './Statuses.js';
import loglevel from 'loglevel-decorator';
import Constants from '../util/Constants.js';

/**
 * Current loaded map with tile positions
 */
 @loglevel
export default class MapState
{
  constructor(pieceState)
  {
    this.maps = {};
    this.currentMap = null;
    this.pieceState = pieceState;
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
        tile.unpassable,
        tile.clearable
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
    let toCheck = [
      center.addX(1),
      center.addX(-1),
      center.addZ(1),
      center.addZ(-1)
    ];

    return this.checkNeighbors(toCheck);
  }

  getKingNeighbors(center)
  {
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

    return this.checkNeighbors(toCheck);
  }

  checkNeighbors(toCheck)
  {
    let ret = [];
    let neighborTile = null;
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

  //Pathfinding copied from client
  findPath(start, end, maxDist, piece)
  {
    var ret = [];
    if(start === end) return ret;

    // The set of nodes already evaluated.
    var closedset = [];

    // The set of tentative nodes to be evaluated, initially containing the start node
    var openset = [ start ];

    // The map of navigated nodes.
    var came_from = {};

    var g_score = {};
    g_score[start] = 0;    // Cost from start along best known path.

    // Estimated total cost from start to goal through y.
    var f_score = {};
    f_score[start] = g_score[start] + this.tileDistance(start.position, end.position);

    let itr = 0;
    while (openset.length > 0) {
      itr++;
      // the node in openset having the lowest f_score[] value
      var current = _.sortBy(openset, x => this.getValueOrMax(f_score,x))[0];
      console.log(`${itr} current`, current);
      console.log(`${itr} open`, openset);
      console.log(`${itr} closed`, closedset);
      if (current.position.equals(end.position)) {
        console.log(`found path`, came_from);
        return this.reconstructPath(came_from, end);
      }

      _.remove(openset, o => o.position.equals(current.position));
      closedset.push(current);

      var neighbors = this.getMovableNeighbors(current, piece, end, false);
      for (let neighborDict of neighbors) {
        var neighbor = neighborDict.value;
        if(closedset.includes(neighbor)){
          continue;
        }

        var tentative_g_score = this.getValueOrMax(g_score,current) + this.tileDistance(current.position, neighbor.position);

        if (!openset.includes(neighbor) || tentative_g_score < this.getValueOrMax(g_score,neighbor)) {
          //check for max dist along path
          if (tentative_g_score > maxDist)
          {
            continue;
          }

          came_from[neighbor] = current;
          g_score[neighbor] = tentative_g_score;
          f_score[neighbor] = this.getValueOrMax(g_score,neighbor) + this.tileDistance(neighbor.position, end.position);
          if (!openset.includes(neighbor)) {
            openset.push(neighbor);
          }
        }
      }
    }

    return null;
  }

  reconstructPath(came_from, current) {
    var total_path = [ current ];
    while(came_from[current] !== undefined){
      current = came_from[current];
      total_path.push(current);
    }
    _.reverse(total_path);
    //remove starting tile
    total_path.shift();
    return total_path;
  }

  getValueOrMax(dict, key)
  {
    if(dict[key] !== undefined) return dict[key];
    return 9999999;
  }

  /// <summary>
  /// Find neighboring tiles that aren't occupied by enemies,
  /// but always include the dest tile for attacking if it's passed
  /// but also make sure not to land on a tile with an occupant if attacking
  /// </summary>
  getMovableNeighbors(center, piece, dest, includeOccupied)
  {
      var ret = this.getNeighbors(center.position)
        .map(n => {
          return {
            key: n,
            value: this.getTile(n)
          }
        });

      //filter tiles that are too high/low to move to & are passable
      ret = ret.filter(t =>
          !t.value.unpassable
          && (this.isHeightPassable(t.value, center) || (piece.statuses & Statuses.Flying) != 0)
      );

      if(!includeOccupied){
          //filter out tiles with enemies on them that aren't the destination
          ret = ret.filter(t =>
              (dest != null && t.key.equals(dest)) ||
              !this.pieceState.pieces.some(m => m.position.tileEquals(t.key) && m.playerId != piece.playerId)
          );

          let destinationOccupied = dest != null && this.pieceState.pieces.some(p => p.position.tileEquals(dest.position));

          //make sure not to consider tiles that would be where the moving pieces lands when it attacks
          ret = ret.filter(t =>
              dest == null
              || dest.position.tileEquals(t.key)
              || !destinationOccupied
              || this.tileDistance(t.key, dest.position) > 1
              || !this.pieceState.pieces.some(p => p.position.tileEquals(t.Key))
          );
      }

      return ret;
  }
}
