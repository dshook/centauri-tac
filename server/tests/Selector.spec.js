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

  t.equal(selector.selectPlayer(1, {left: 'PLAYER'}), 1, 'Player is equal to itself');
  t.equal(selector.selectPlayer(1, {left: 'OPPONENT'}), 2, 'Select opponent');

});

//Not worrying about the "randomness" of the result since that's provided through lodash
//which should be tested enough for us.  Testing that the selector returns the right class
//of whatever should be taken care of by the multi piece selector tests
test('Select a piece', t => {
  let selectorTemplate = {
    random: true,
    selector:{
      left : ''
    }
  };
  let selectors = [
    'FRIENDLY',
    'ENEMY',
    'MINION'
  ];
  t.plan(selectors.length);
  let selector = new Selector(players, pieceStateMix);

  for(let selectString of selectors){
    selectorTemplate.selector.left = selectString;
    let selection = selector.selectPiece(1, selectorTemplate);
    t.ok(selection instanceof GamePiece, 'Selection got a gamepiece back for ' + selectString);
  }
});

test('Characters', t => {
  t.plan(3);
  let selector = new Selector(players, pieceStateMix);
  let selection = selector.selectPieces(1, {left: 'CHARACTER' });

  t.ok(Array.isArray(selection), 'Got back an Array');
  t.equal(selection.length, pieceStateMix.pieces.length, 'Got back all the pieces'); 
  t.ok(selection[0] instanceof GamePiece, 'First element is a game piece');
});

// test('Friendly Characters', t => {
//   t.plan(6);
//   let select = 
//     {
//       left: 'FRIENDLY',
//       op: '&',
//       right: 'CHARACTER'
//     };
//   let selector = new Selector(players, pieceStateMix);
//   let selection = selector.selectPieces(1, select);
  
//   t.ok(Array.isArray(selection), 'Got back an Array');
//   t.equal(selection.length, 3, 'Got only friendly Pieces'); 
//   t.ok(selection[0] instanceof GamePiece, 'First element is a game piece');
//   t.equal(selection[0].playerId, 1, 'First piece is for the right player');

//   let emptySelector = new Selector(players, noPieces);
//   let emptySelection = emptySelector.selectPieces(1, select);
//   t.ok(Array.isArray(emptySelection), 'Got back an Array');
//   t.equal(emptySelection.length, 0, 'Got nothin'); 
// });

// test('Enemy Characters', t => {
//   t.plan(8);
//   let select = 'ENEMY_CHARACTERS';
//   let selector = new Selector(players, pieceStateMix);
//   let selection = selector.selectPieces(1, select);
  
//   t.ok(Array.isArray(selection), 'Got back an Array');
//   t.equal(selection.length, 3, 'Got only enemy Pieces'); 
//   t.ok(selection[0] instanceof GamePiece, 'First element is a game piece');
//   t.equal(selection[0].playerId, 2, 'First piece is for the enemy player');

//   let emptySelector = new Selector(players, noPieces);
//   let emptySelection = emptySelector.selectPieces(1, select);
//   t.ok(Array.isArray(emptySelection), 'Got back an Array');
//   t.equal(emptySelection.length, 0, 'Got nothin'); 

//   let heroSelector = new Selector(players, heroesOnly);
//   let heroesSelection = heroSelector.selectPieces(1, select);
//   t.equal(heroesSelection.length, 1, 'Selected 1 hero');
//   t.equal(heroesSelection[0].tags[0], 'Hero', 'Selected piece is a hero');
// });

// test('Enemy Minions', t => {
//   t.plan(7);
//   let select = 'ENEMY_MINIONS';
//   let selector = new Selector(players, pieceStateMix);
//   let selection = selector.selectPieces(1, select);
  
//   t.ok(Array.isArray(selection), 'Got back an Array');
//   t.equal(selection.length, 2, 'Got only enemy Pieces'); 
//   t.ok(selection[0] instanceof GamePiece, 'First element is a game piece');
//   t.equal(selection[0].playerId, 2, 'First piece is for the right player');
//   t.equal(selection[0].tags[0], 'Minion', 'Selected piece is a minion');

//   let emptySelector = new Selector(players, noPieces);
//   let emptySelection = emptySelector.selectPieces(1, select);
//   t.ok(Array.isArray(emptySelection), 'Got back an Array');
//   t.equal(emptySelection.length, 0, 'Got nothin'); 
// });

// test('Friendly Minions', t => {
//   t.plan(7);
//   let select = 'FRIENDLY_MINIONS';
//   let selector = new Selector(players, pieceStateMix);
//   let selection = selector.selectPieces(1, select);
  
//   t.ok(Array.isArray(selection), 'Got back an Array');
//   t.equal(selection.length, 2, 'Got only friendly Pieces'); 
//   t.ok(selection[0] instanceof GamePiece, 'First element is a game piece');
//   t.equal(selection[0].playerId, 1, 'First piece is for the right player');
//   t.equal(selection[0].tags[0], 'Minion', 'Selected piece is a minion');

//   let emptySelector = new Selector(players, noPieces);
//   let emptySelection = emptySelector.selectPieces(1, select);
//   t.ok(Array.isArray(emptySelection), 'Got back an Array');
//   t.equal(emptySelection.length, 0, 'Got nothin'); 
// });


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
