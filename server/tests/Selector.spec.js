import test from 'tape';
import Selector from '../game/ctac/cardlang/Selector.js';
import Statuses from '../game/ctac/models/Statuses.js';
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

//dummy activating and self pieces
var activatingPiece = new GamePiece();
var selfPiece = new GamePiece();

//various setups for game state
var pieceStateMix = new PieceState();
spawnPiece(pieceStateMix, 1, 1);
spawnPiece(pieceStateMix, 2, 1);
spawnPiece(pieceStateMix, 3, 1);
spawnPiece(pieceStateMix, 1, 2);
spawnPiece(pieceStateMix, 2, 2);
spawnPiece(pieceStateMix, 3, 2);

var heroesOnly = new PieceState();
spawnPiece(heroesOnly, 1, 1);
spawnPiece(heroesOnly, 1, 2);

var noPieces = new PieceState();

var statusPieces = new PieceState();
spawnPiece(statusPieces, 1, 1);
spawnPiece(statusPieces, 2, 1);
spawnPiece(statusPieces, 28, 1);
spawnPiece(statusPieces, 1, 2);
spawnPiece(statusPieces, 2, 2);
spawnPiece(statusPieces, 28, 2);

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
      left : 'Selector.spec.js'
    }
  };
  let selectors = [
    'FRIENDLY',
    'ENEMY',
    'MINION',
    'HERO'
  ];
  t.plan(selectors.length * 2);
  let selector = new Selector(players, pieceStateMix);

  for(let selectString of selectors){
    selectorTemplate.selector.left = selectString;
    let selection = selector.selectPieces(1, selectorTemplate, {selfPiece});
    t.equal(selection.length, 1, 'Got a single piece back');
    t.ok(selection[0] instanceof GamePiece, 'Selection got a gamepiece back for ' + selectString);
  }
});

test('Random none selected', t => {
  t.plan(2);

  let selectorTemplate = {
    random: true,
    selector:{
      left : 'MINION'
    }
  };
  let selector = new Selector(players, heroesOnly);
  let selection = selector.selectPieces(1, selectorTemplate, {selfPiece});
  t.ok(Array.isArray(selection), 'Got back an Array');
  t.equal(selection.length, 0, 'Got nothing back');
});

test('Characters', t => {
  t.plan(3);
  let selector = new Selector(players, pieceStateMix);
  let selection = selector.selectPieces(1, {left: 'CHARACTER' }, {selfPiece});

  t.ok(Array.isArray(selection), 'Got back an Array');
  t.equal(selection.length, pieceStateMix.pieces.length, 'Got back all the pieces');
  t.ok(selection[0] instanceof GamePiece, 'First element is a game piece');
});

test('Friendly Characters Intersection', t => {
  t.plan(6);
  let select =
    {
      left: 'FRIENDLY',
      op: '&',
      right: 'CHARACTER'
    };
  let selector = new Selector(players, pieceStateMix);
  let selection = selector.selectPieces(1, select, {selfPiece});

  t.ok(Array.isArray(selection), 'Got back an Array');
  t.equal(selection.length, 3, 'Got only friendly Pieces');
  t.ok(selection[0] instanceof GamePiece, 'First element is a game piece');
  t.equal(selection[0].playerId, 1, 'First piece is for the right player');

  let emptySelector = new Selector(players, noPieces);
  let emptySelection = emptySelector.selectPieces(1, select, {selfPiece});
  t.ok(Array.isArray(emptySelection), 'Got back an Array');
  t.equal(emptySelection.length, 0, 'Got nothin');
});

test('Enemy Characters by Difference', t => {
  t.plan(9);
  let select =
    {
      left: 'CHARACTER',
      op: '-',
      right: 'FRIENDLY'
    };
  let selector = new Selector(players, pieceStateMix);
  let selection = selector.selectPieces(1, select, {selfPiece});

  t.ok(Array.isArray(selection), 'Got back an Array');
  t.equal(selection.length, 3, 'Got only enemy Pieces');
  t.ok(selection[0] instanceof GamePiece, 'First element is a game piece');
  t.equal(selection[0].playerId, 2, 'First piece is for the enemy player');

  let emptySelector = new Selector(players, noPieces);
  let emptySelection = emptySelector.selectPieces(1, select, {selfPiece});
  t.ok(Array.isArray(emptySelection), 'Got back an Array');
  t.equal(emptySelection.length, 0, 'Got nothin');

  let heroSelector = new Selector(players, heroesOnly);
  let heroesSelection = heroSelector.selectPieces(1, select, {selfPiece});
  t.equal(heroesSelection.length, 1, 'Selected 1 hero');
  t.equal(heroesSelection[0].tags[0], 'Hero', 'Selected piece is a hero');
  t.equal(heroesSelection[0].playerId, 2, 'Selected enemy hero');
});

test('Enemy Minions', t => {
  t.plan(7);
  let select =
    {
      left: 'ENEMY',
      op: '&',
      right: 'MINION'
    };
  let selector = new Selector(players, pieceStateMix);
  let selection = selector.selectPieces(1, select, {selfPiece});

  t.ok(Array.isArray(selection), 'Got back an Array');
  t.equal(selection.length, 2, 'Got only enemy Pieces');
  t.ok(selection[0] instanceof GamePiece, 'First element is a game piece');
  t.equal(selection[0].playerId, 2, 'First piece is for the right player');
  t.equal(selection[0].tags[0], 'Minion', 'Selected piece is a minion');

  let emptySelector = new Selector(players, noPieces);
  let emptySelection = emptySelector.selectPieces(1, select, {selfPiece});
  t.ok(Array.isArray(emptySelection), 'Got back an Array');
  t.equal(emptySelection.length, 0, 'Got nothin');
});

test('Peace treaty union', t => {
  t.plan(5);
  let select =
    {
      left: 'ENEMY',
      op: '|',
      right: 'FRIENDLY'
    };
  let selector = new Selector(players, pieceStateMix);
  let selection = selector.selectPieces(1, select, {selfPiece});

  t.ok(Array.isArray(selection), 'Got back an Array');
  t.equal(selection.length, pieceStateMix.pieces.length, 'Got back all pieces');
  t.ok(selection[0] instanceof GamePiece, 'First element is a game piece');

  let emptySelector = new Selector(players, noPieces);
  let emptySelection = emptySelector.selectPieces(1, select, {selfPiece});
  t.ok(Array.isArray(emptySelection), 'Got back an Array');
  t.equal(emptySelection.length, 0, 'Got nothin');
});

test('Nested condition', t => {
  t.plan(4);
  let select =
    {
      left: {
        left: 'FRIENDLY',
        op: '&',
        right: 'CHARACTER'
      },
      op: '-',
      right: 'HERO'
    };
  let selector = new Selector(players, pieceStateMix);
  let selection = selector.selectPieces(1, select, {selfPiece});

  t.ok(Array.isArray(selection), 'Got back an Array');
  t.equal(selection.length, 2, 'Got back friendly characters who arent a hero');
  t.ok(selection[0] instanceof GamePiece, 'First element is a game piece');
  t.equal(selection[0].tags[0], 'Minion', 'Selected piece is a minion');

});

test('Self Selector', t => {
  t.plan(5);
  let select =
    {
      left: 'SELF',
    };

  let selfPiece = pieceStateMix.pieces[pieceStateMix.pieces.length - 1];
  let triggerPiece = pieceStateMix.pieces[pieceStateMix.pieces.length - 2];

  let selector = new Selector(players, pieceStateMix);
  let selection = selector.selectPieces(1, select, {selfPiece, activatingPiece: triggerPiece});

  t.ok(Array.isArray(selection), 'Got back an Array');
  t.equal(selection.length, 1, 'Got back just self');
  t.ok(selection[0] instanceof GamePiece, 'First element is a game piece');
  t.equal(selection[0], selfPiece, 'Selected piece is self piece');
  t.notEqual(selection[0], triggerPiece, 'Selected piece is not triggering piece');
});


test('Possible Targets', t => {
  t.plan(7);
  let nonTargetSelect = {
    left: 'FRIENDLY',
    op: '&',
    right: 'CHARACTER'
  };

  let selector = new Selector(players, pieceStateMix);
  let nonTargetSelection = selector.selectPossibleTargets(1, nonTargetSelect);

  t.ok(Array.isArray(nonTargetSelection), 'Got back an Array');
  t.equal(nonTargetSelection.length, 0, 'Nothing to target here');

  let randomNonTarget = {
    random: true,
    selector:{
      left : 'MINION'
    }
  };

  let randoTargetSelection = selector.selectPossibleTargets(1, randomNonTarget);

  t.ok(Array.isArray(randoTargetSelection), 'Got back an Array');
  t.equal(randoTargetSelection.length, 0, 'Nothing to target here');

  let targetSelector = {
    left: 'FRIENDLY',
    op: '&',
    right: 'TARGET'
  };

  let targetSelect = selector.selectPossibleTargets(1, targetSelector);

  t.ok(Array.isArray(targetSelect), 'Got back an Array');
  t.equal(targetSelect.length, 3, 'Got back 3 targets');
  t.equal(targetSelect[0].playerId, 1, 'Got back friendly target');
});

test('Status Selector', t => {
  t.plan(5);
  let select =
    {
      left: 'SHIELD',
      op: '&',
      right: 'FRIENDLY'
    };

  let selector = new Selector(players, statusPieces);
  let selection = selector.selectPieces(1, select, {selfPiece});

  t.ok(Array.isArray(selection), 'Got back an Array');
  t.equal(selection.length, 1, 'Got back one shielded');
  t.ok(selection[0] instanceof GamePiece, 'First element is a game piece');
  t.equal(selection[0].playerId, 1, 'Friendly minion');
  t.equal(selection[0].statuses, Statuses.Shield, 'Selected piece has shield');
});

test('Selector with comparison expression', t => {
  t.plan(5);
  let select =
    {
      left: {
        left: "ENEMY",
        op: "&",
        right: {
          compareExpression: true,
          left: {
            eNumber: true,
            attributeSelector: {
              left: "ENEMY",
              op: "&",
              right: "MINION"
            },
            attribute: "attack"
          },
          op: "<",
          right: 3
        }
      },
      op: "&",
      right: "MINION"
    };

  let selector = new Selector(players, pieceStateMix);
  let selection = selector.selectPieces(1, select, {selfPiece});

  t.ok(Array.isArray(selection), 'Got back an Array');
  t.equal(selection.length, 1, 'Got back only one piece');
  t.ok(selection[0] instanceof GamePiece, 'First element is a game piece');
  t.equal(selection[0].playerId, 2, 'Enemy minion');
  t.ok(selection[0].attack < 3, 'Attack is less than selector specified');
});

function spawnPiece(pieceState, cardTemplateId, playerId, addToState = true){
    var newPiece = pieceState.newFromCard(cardDirectory, cardTemplateId, playerId, null);

    if(addToState){
      pieceState.add(newPiece);
    }
    return newPiece;
}
