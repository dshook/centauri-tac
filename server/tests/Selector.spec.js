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

var empty = new PieceState();

test('Select Player', t => {
  t.plan(2);
  let selector = new Selector(players, heroesOnly);

  t.equal(selector.selectPlayer(1, 'PLAYER'), 1, 'Player is equal to itself');
  t.equal(selector.selectPlayer(1, 'OPPONENT'), 2, 'Select opponent');

});

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
