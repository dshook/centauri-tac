import test from 'tape';
import MapState from '../game/ctac/models/MapState.js';
import Position from '../game/ctac/models/Position.js';
import Tile from     '../game/ctac/models/Tile.js';
import PieceState from '../game/ctac/models/PieceState.js';
import CardDirectory from '../game/ctac/models/CardDirectory.js';
import cubeland from '../../maps/cubeland.json';

function positionSort(a, b) {
  return 10000 * (a.x - b.x) + (a.z - b.z);
}

function positionArrayEquals(a, b){
  if(a.length != b.length){
    // console.log('actual', a);
    // console.log('expected', b);
    return false;
  }

  a.sort(positionSort);
  b.sort(positionSort);

  // console.log('actual', a);
  // console.log('expected', b);

  for(let i = 0; i < a.length; i++){
    if( !a[i].equals(b[i]) ) return false;
  }
  return true;
}

function createTiles(x, z)
{
  var tiles = [];
  for (let i = 0; i < x; i++)
  {
    for (let j = 0; j < z; j++)
    {
      var transform = new Position(i, 0, j);
      tiles.push({
        transform,
        unpassable: false
      });
    }
  }
  return tiles;
}

var cardDirectory = new CardDirectory({cardSets: ['test']});


function spawnPiece(pieceState, cardTemplateId, playerId, addToState = true, position = null){
  var newPiece = pieceState.newFromCard(cardDirectory, cardTemplateId, playerId, position);

  if(addToState){
    pieceState.add(newPiece);
  }
  return newPiece;
}

function mockPiece(pieceState, position, currentPlayerHasControl){
  return spawnPiece(pieceState, 105, currentPlayerHasControl ? 1 : 2, true, position);
}

test('tile equals', t => {
  var mapState = new MapState();
  mapState.add(cubeland, true);
  t.plan(3);

  var corner = mapState.getTile(new Position(0, 0, 0));
  t.ok(corner instanceof Tile, 'Got a tile');
  t.ok(corner.position instanceof Position, 'Tile has position');
  t.ok(corner.position.tileEquals(new Position(0, 0, 0)), 'Tile position is the corner');
});

test('get neighbors', t => {
  var mapState = new MapState();
  mapState.add(cubeland, true);
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
  mapState.add(cubeland, true);
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
  mapState.add(cubeland, true);
  t.plan(1);

  let edgeCrossTiles = mapState.getCrossTiles(new Position(1, 0, 1), 2);
  let expectedEdgeCrossTiles = [
    mapState.getTile(new Position(1,0,1)).position,
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
  mapState.add(cubeland, true);
  t.plan(2);

  let diagonalTiles = mapState.getLineTiles(new Position(1, 0, 1), new Position(0, 0, 0), 2, true);
  let expectedDiagonalTiles = [
    mapState.getTile(new Position(1,0,1)).position,
    mapState.getTile(new Position(0,0,0)).position,
    mapState.getTile(new Position(2,0,2)).position,
    mapState.getTile(new Position(3,0,3)).position,
  ];
  t.ok(positionArrayEquals(diagonalTiles, expectedDiagonalTiles), 'Got expected diagonal tiles');

  let lineTiles = mapState.getLineTiles(new Position(2, 0, 2), new Position(2, 0, 3), 3, false);
  let expectedLineTiles = [
    mapState.getTile(new Position(2,0,2)).position,
    mapState.getTile(new Position(2,0,3)).position,
    mapState.getTile(new Position(2,0,4)).position,
    mapState.getTile(new Position(2,0,5)).position,
  ];
  t.ok(positionArrayEquals(lineTiles, expectedLineTiles), 'Got expected line tiles');
});

test('Map state handles stacked tiles', t => {
  var mapState = new MapState();
  mapState.add(cubeland, true);
  t.plan(3);

  t.equals(mapState.map.tiles.length, 121, 'Map did not register stacked tiles');
  var corner = mapState.getTile(new Position(10, 0, 10));
  t.ok(corner instanceof Tile, 'Got a tile');
  t.ok(corner.position.equals(new Position(10, 3, 10)), 'Tile position is the highest one');
});


test('Find Path', t => {

  let pieceStateMix = new PieceState();
  mockPiece(pieceStateMix, new Position(1, 0, 2), false),
  mockPiece(pieceStateMix, new Position(2, 0, 3), true),
  mockPiece(pieceStateMix, new Position(3, 0, 2), true),
  mockPiece(pieceStateMix, new Position(4, 0, 2), true),
  mockPiece(pieceStateMix, new Position(5, 0, 2), false)

  var mapState = new MapState(pieceStateMix);
  mapState.add({
      name: "test map",
      maxPlayers: 2,
      tiles: createTiles(10, 10),
      startingPositions: []
  }, true);

  var movingPiece = spawnPiece(pieceStateMix, 105, 1, false, new Position(2,0,2));
  var start = mapState.getTile(new Position(2,0,2));

  //test to walk through friendly
  var end = mapState.getTile(new Position(2,0,4));
  var tilePath = mapState.findPath(start, end, 2, movingPiece);
  var expectedTiles = [
    mapState.getTile(new Position(2, 0, 3)).position,
    mapState.getTile(new Position(2, 0, 4)).position
  ];
  t.ok(positionArrayEquals(tilePath.map(t => t.position), expectedTiles), 'Got expected path');

  //test to walk around enemy
  var enemyEnd = mapState.getTile(new Position(0,0,2));

  var enemyPath = mapState.findPath(start, enemyEnd, 4, movingPiece);
  var expectedEnemyPath = [
    mapState.getTile(new Position(2, 0, 3)).position,
    mapState.getTile(new Position(1, 0, 3)).position,
    mapState.getTile(new Position(0, 0, 3)).position,
    mapState.getTile(new Position(0, 0, 2)).position,
  ];
  t.ok(positionArrayEquals(enemyPath.map(t => t.position), expectedEnemyPath), 'Got expected enemy path');

  //test to attack enemy but not land on a friendly
  var passThroughEnd = mapState.getTile(new Position(5,0,2));
  var passTilePath = mapState.findPath(start, passThroughEnd, 6, movingPiece);
  var passExpectedTiles = [
      mapState.getTile(new Position(3, 0, 2)).position,
      mapState.getTile(new Position(3, 0, 3)).position,
      mapState.getTile(new Position(4, 0, 3)).position,
      mapState.getTile(new Position(5, 0, 3)).position,
      mapState.getTile(new Position(5, 0, 2)).position
  ];
  t.ok(positionArrayEquals(passTilePath.map(t => t.position), passExpectedTiles), 'Got expected passing through path');

  t.end();
});
