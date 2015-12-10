import test from 'tape';
import Selector from '../game/ctac/cardlang/Selector.js';
import GamePiece from '../game/ctac/models/GamePiece.js';
import PieceState from '../game/ctac/models/PieceState.js';
import Player from 'models/Player';
import requireDir from 'require-dir';
import CardDirectory from '../game/ctac/models/CardDirectory.js';

//init the dependencies for the selector
var cardRequires = requireDir('../../cards/');
var cardDirectory = new CardDirectory();

for(let cardFileName in cardRequires){
  let card = cardRequires[cardFileName];
  cardDirectory.add(card);
}

var player1 = new Player(1);
var player2 = new Player(2);
var players = [player1, player2];

//various setups for game state
var pieceStateMix = new PieceState();
spawnPiece(pieceStateMix, 1, 1);
spawnPiece(pieceStateMix, 2, 1);
spawnPiece(pieceStateMix, 3, 1);
spawnPiece(pieceStateMix, 1, 2);
spawnPiece(pieceStateMix, 2, 2);
spawnPiece(pieceStateMix, 3, 2);

var heroesOnly = new PieceState();
spawnPiece(heroesOnly, 2, 1);
spawnPiece(heroesOnly, 2, 2);

var noPieces = new PieceState();

test('Select Player', t => {
  t.plan(2);
  let selector = new Selector(players, heroesOnly);

  t.equal(selector.selectPlayer(1, 'PLAYER'), 1, 'Player is equal to itself');
  t.equal(selector.selectPlayer(1, 'OPPONENT'), 2, 'Select opponent');

});

//Not worrying about the "randomness" of the result since that's provided through lodash
//which should be tested enough for us.  Testing that the selector returns the right class
//of whatever should be taken care of by the multi piece selector tests
test('Select a piece', t => {
  let selectors = [
    'RANDOM_CHARACTER',
    'RANDOM_FRIENDLY_CHARACTER',
    'RANDOM_ENEMY_CHARACTER',
    'RANDOM_ENEMY_MINION',
    'RANDOM_FRIENDLY_MINION'
  ];
  t.plan(selectors.length);
  let selector = new Selector(players, pieceStateMix);

  for(let selectString of selectors){
    let selection = selector.selectPiece(1, selectString);
    t.ok(selection instanceof GamePiece, 'Selection got a gamepiece back for ' + selectString);
  }
});

test('Characters', t => {
  t.plan(3);
  let selector = new Selector(players, pieceStateMix);
  let selection = selector.selectPieces(1, 'CHARACTERS');

  t.ok(Array.isArray(selection), 'Got back an Array');
  t.equal(selection.length, pieceStateMix.pieces.length, 'Got back all the pieces'); 
  t.ok(selection[0] instanceof GamePiece, 'First element is a game piece');
});

test('Friendly Characters', t => {
  t.plan(6);
  let selector = new Selector(players, pieceStateMix);
  let selection = selector.selectPieces(1, 'FRIENDLY_CHARACTERS');
  
  t.ok(Array.isArray(selection), 'Got back an Array');
  t.equal(selection.length, 3, 'Got only friendly Pieces'); 
  t.ok(selection[0] instanceof GamePiece, 'First element is a game piece');
  t.equal(selection[0].playerId, 1, 'First piece is for the right player');

  let emptySelector = new Selector(players, noPieces);
  let emptySelection = emptySelector.selectPieces(1, 'FRIENDLY_CHARACTERS');
  t.ok(Array.isArray(emptySelection), 'Got back an Array');
  t.equal(emptySelection.length, 0, 'Got nothin'); 
});


function spawnPiece(pieceState, cardId, playerId){
    let cardPlayed = cardDirectory.directory[cardId];

    var newPiece = new GamePiece();
    //newPiece.position = action.position;
    newPiece.playerId = playerId;
    newPiece.cardId = cardId;
    newPiece.attack = cardPlayed.attack;
    newPiece.health = cardPlayed.health;
    newPiece.baseAttack = cardPlayed.attack;
    newPiece.baseHealth = cardPlayed.health;
    newPiece.movement = cardPlayed.movement;
    newPiece.baseMovement = cardPlayed.movement;
    newPiece.tags = cardPlayed.tags;

    pieceState.add(newPiece);
}
