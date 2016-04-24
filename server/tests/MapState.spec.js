import test from 'tape';
import MapState from '../game/ctac/models/MapState.js';
import Position from '../game/ctac/models/Position.js';
import Tile from     '../game/ctac/models/Tile.js';
import cubeland from '../../maps/cubeland.json';

function positionSort(a, b) {
  return 10000 * (a.x - b.x) + (a.z - b.z);
}

function positionArrayEquals(a, b){
  if(a.length != b.length){
    console.log('actual', a);
    console.log('expected', b);
    return false;
  }

  a.sort(positionSort);
  b.sort(positionSort);

  console.log('actual', a);
  console.log('expected', b);

  for(let i = 0; i < a.length; i++){
    if( !a[i].equals(b[i]) ) return false;
  }
  return true;
}

test('tile equals', t => {
  var mapState = new MapState();
  mapState.add(cubeland);
  t.plan(3);

  var corner = mapState.getTile(new Position(0, 0, 0));
  t.ok(corner instanceof Tile, 'Got a tile');
  t.ok(corner.position instanceof Position, 'Tile has position');
  t.ok(corner.position.tileEquals(new Position(0, 0, 0)), 'Tile position is the corner');
});

test('get neighbors', t => {
  var mapState = new MapState();
  mapState.add(cubeland);
  t.plan(2);

  let cornerNeighbors = mapState.getNeighbors(new Position(0, 0, 0));
  let expectedCorner = [
    mapState.getTile(new Position(1,0,0)).position,
    mapState.getTile(new Position(0,0,1)).position
  ];
  t.ok(positionArrayEquals(cornerNeighbors, expectedCorner), 'Got right corner neighbors');

  let middleNeighbors = mapState.getNeighbors(new Position(1, 0, 1));
  let expectedMiddle = [
    mapState.getTile(new Position(2,0,1)).position,
    mapState.getTile(new Position(0,0,1)).position,
    mapState.getTile(new Position(1,0,2)).position,
    mapState.getTile(new Position(1,0,0)).position
  ];
  t.ok(positionArrayEquals(middleNeighbors, expectedMiddle), 'Got center neighbors');

});

test('Get Tiles in Radius', t => {
  var mapState = new MapState();
  mapState.add(cubeland);
  t.plan(2);

  let zeroTileRadius = mapState.getTilesInRadius(new Position(1, 0, 1), 0);
  let expectedZeros = [
    mapState.getTile(new Position(1,0,1)).position,
  ];
  t.ok(positionArrayEquals(zeroTileRadius, expectedZeros), 'Got same tile back for 0 radius');

  let oneTileRadus = mapState.getTilesInRadius(new Position(1, 0, 1), 1);
  let expectedOnes = [
    mapState.getTile(new Position(1,0,1)).position,
    mapState.getTile(new Position(2,0,1)).position,
    mapState.getTile(new Position(0,0,1)).position,
    mapState.getTile(new Position(1,0,2)).position,
    mapState.getTile(new Position(1,0,0)).position,
  ];
  t.ok(positionArrayEquals(oneTileRadus, expectedOnes), 'Got expected for 1 radius');

});

test('Get cross Tiles', t => {
  var mapState = new MapState();
  mapState.add(cubeland);
  t.plan(1);

  let edgeCrossTiles = mapState.getCrossTiles(new Position(1, 0, 1), 2);
  let expectedEdgeCrossTiles = [
    mapState.getTile(new Position(0,0,1)).position,
    mapState.getTile(new Position(1,0,0)).position,
    mapState.getTile(new Position(1,0,2)).position,
    mapState.getTile(new Position(2,0,1)).position,
    mapState.getTile(new Position(3,0,1)).position,
    mapState.getTile(new Position(1,0,3)).position,
  ];
  t.ok(positionArrayEquals(edgeCrossTiles, expectedEdgeCrossTiles), 'Got expected cross tiles');
});

test('Get line Tiles', t => {
  var mapState = new MapState();
  mapState.add(cubeland);
  t.plan(2);

  let diagonalTiles = mapState.getLineTiles(new Position(1, 0, 1), new Position(0, 0, 0), 2, true);
  let expectedDiagonalTiles = [
    mapState.getTile(new Position(0,0,0)).position,
    mapState.getTile(new Position(2,0,2)).position,
    mapState.getTile(new Position(3,0,3)).position,
  ];
  t.ok(positionArrayEquals(diagonalTiles, expectedDiagonalTiles), 'Got expected diagonal tiles');

  let lineTiles = mapState.getLineTiles(new Position(2, 0, 2), new Position(2, 0, 3), 3, false);
  let expectedLineTiles = [
    mapState.getTile(new Position(2,0,3)).position,
    mapState.getTile(new Position(2,0,4)).position,
    mapState.getTile(new Position(2,0,5)).position,
  ];
  t.ok(positionArrayEquals(lineTiles, expectedLineTiles), 'Got expected line tiles');
});
