import test from 'tape';
import CardEvaluator from '../game/ctac/cardlang/CardEvaluator.js';
import Selector from '../game/ctac/cardlang/Selector.js';
import GamePiece from '../game/ctac/models/GamePiece.js';
import PieceState from '../game/ctac/models/PieceState.js';
import DrawCard from '../game/ctac/actions/DrawCard.js';
import PieceHealthChange from '../game/ctac/actions/PieceHealthChange.js';
import PieceAttributeChange from '../game/ctac/actions/PieceAttributeChange.js';
import Player from 'models/Player';
import ActionQueue from 'action-queue';
import requireDir from 'require-dir';
import CardDirectory from '../game/ctac/models/CardDirectory.js';

//init the dependencies for the evaluator and selector
var cardRequires = requireDir('../../cards/');
var cardDirectory = new CardDirectory();
for(let cardFileName in cardRequires){
  let card = cardRequires[cardFileName];
  cardDirectory.add(card);
}

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

var player1 = new Player(1);
var player2 = new Player(2);
var players = [player1, player2];


test('Basic Draw card', t => {
  let pieceStateMix = new PieceState();
  spawnPiece(pieceStateMix, 1, 1);
  spawnPiece(pieceStateMix, 2, 1);
  spawnPiece(pieceStateMix, 3, 1);
  spawnPiece(pieceStateMix, 9, 1);
  spawnPiece(pieceStateMix, 1, 2);
  spawnPiece(pieceStateMix, 2, 2);
  spawnPiece(pieceStateMix, 3, 2);

  t.plan(4);
  let queue = new ActionQueue();
  let selector = new Selector(players, pieceStateMix);
  let cardEval = new CardEvaluator(queue, selector, cardDirectory, pieceStateMix);

  let testBot = pieceStateMix.pieces.filter(p => p.playerId == 1 && p.cardId == 3)[0];
  t.ok(testBot, 'Found test bot');

  cardEval.evaluatePieceEvent('playMinion', testBot);

  t.equal(queue._actions.length, 2, '2 Actions in the queue');
  t.ok(queue._actions[0] instanceof DrawCard, 'First action is Draw Card');
  t.ok(queue._actions[1] instanceof DrawCard, 'Second action is Draw Card');
});

test('Basic Hit action', t => {
  let pieceStateMix = new PieceState();
  spawnPiece(pieceStateMix, 1, 1);
  spawnPiece(pieceStateMix, 2, 1);
  spawnPiece(pieceStateMix, 3, 1);
  spawnPiece(pieceStateMix, 9, 1);
  spawnPiece(pieceStateMix, 1, 2);
  spawnPiece(pieceStateMix, 2, 2);
  spawnPiece(pieceStateMix, 3, 2);

  t.plan(4);
  let queue = new ActionQueue();
  let selector = new Selector(players, pieceStateMix);
  let cardEval = new CardEvaluator(queue, selector, cardDirectory, pieceStateMix);

  let testBot = pieceStateMix.pieces.filter(p => p.playerId == 1 && p.cardId == 9)[0];
  t.ok(testBot, 'Found writhing bunch');

  cardEval.evaluatePieceEvent('playMinion', testBot);

  t.equal(queue._actions.length, 2, '2 Actions in the queue');
  t.ok(queue._actions[0] instanceof PieceHealthChange, 'First action is Hit');
  t.ok(queue._actions[1] instanceof PieceHealthChange, 'Second action is Hit');
});

test('Damaged with selector', t => {
  t.plan(4);

  let pieceState = new PieceState();
  spawnPiece(pieceState, 1, 1);
  spawnPiece(pieceState, 1, 2);
  spawnPiece(pieceState, 12, 1);
  spawnPiece(pieceState, 5, 2);

  let queue = new ActionQueue();
  let selector = new Selector(players, pieceState);
  let cardEval = new CardEvaluator(queue, selector, cardDirectory, pieceState);

  let friendlyHero = pieceState.pieces.filter(p => p.playerId == 1 && p.cardId == 1)[0];
  t.ok(friendlyHero, 'Found friendly hero');

  cardEval.evaluatePieceEvent('damaged', friendlyHero);

  t.equal(queue._actions.length, 1, '1 Actions in the queue');
  let action = queue._actions[0];
  t.ok(action instanceof DrawCard, 'First action is Draw Card');
  t.equal(action.playerId, 1, 'Drew card for right player');

});

test('Set attribute', t => {
  let pieceStateMix = new PieceState();
  spawnPiece(pieceStateMix, 1, 1);
  spawnPiece(pieceStateMix, 2, 1);
  spawnPiece(pieceStateMix, 3, 1);
  spawnPiece(pieceStateMix, 10, 1);
  spawnPiece(pieceStateMix, 1, 2);
  spawnPiece(pieceStateMix, 2, 2);
  spawnPiece(pieceStateMix, 3, 2);

  t.plan(3);
  let queue = new ActionQueue();
  let selector = new Selector(players, pieceStateMix);
  let cardEval = new CardEvaluator(queue, selector, cardDirectory, pieceStateMix);

  let platypus = pieceStateMix.pieces.filter(p => p.playerId == 1 && p.cardId == 10)[0];
  t.ok(platypus, 'Found platypus');

  cardEval.evaluatePieceEvent('playMinion', platypus);

  t.equal(queue._actions.length, 1, '1 Action in the queue');
  t.ok(queue._actions[0] instanceof PieceAttributeChange, 'First action is Set attribute');
});

test('Heal on damaged', t => {
  let pieceStateMix = new PieceState();
  spawnPiece(pieceStateMix, 1, 1);
  spawnPiece(pieceStateMix, 2, 1);
  spawnPiece(pieceStateMix, 3, 1);
  spawnPiece(pieceStateMix, 11, 1);
  spawnPiece(pieceStateMix, 1, 2);
  spawnPiece(pieceStateMix, 2, 2);
  spawnPiece(pieceStateMix, 3, 2);

  t.plan(4);
  let queue = new ActionQueue();
  let selector = new Selector(players, pieceStateMix);
  let cardEval = new CardEvaluator(queue, selector, cardDirectory, pieceStateMix);

  let synth = pieceStateMix.pieces.filter(p => p.playerId == 1 && p.cardId == 11)[0];
  t.ok(synth, 'Found synth');

  cardEval.evaluatePieceEvent('damaged', synth);

  t.equal(queue._actions.length, 1, '1 Action in the queue');
  t.ok(queue._actions[0] instanceof PieceHealthChange, 'First action is Health change');
  t.ok(queue._actions[0].change > 0, 'Healing not hitting');
});

test('Attacks event', t => {
  let pieceStateMix = new PieceState();
  spawnPiece(pieceStateMix, 1, 1);
  spawnPiece(pieceStateMix, 2, 1);
  spawnPiece(pieceStateMix, 3, 1);
  spawnPiece(pieceStateMix, 13, 1);
  spawnPiece(pieceStateMix, 1, 2);
  spawnPiece(pieceStateMix, 2, 2);
  spawnPiece(pieceStateMix, 3, 2);

  t.plan(4);
  let queue = new ActionQueue();
  let selector = new Selector(players, pieceStateMix);
  let cardEval = new CardEvaluator(queue, selector, cardDirectory, pieceStateMix);

  let spore = pieceStateMix.pieces.filter(p => p.playerId == 1 && p.cardId == 13)[0];
  t.ok(spore, 'Found spore');

  cardEval.evaluatePieceEvent('attacks', spore);

  t.equal(queue._actions.length, 1, '1 Action in the queue');
  t.ok(queue._actions[0] instanceof PieceHealthChange, 'First action is Health change');
  t.ok(queue._actions[0].change < 0, 'Hit action');
});

test('Card drawn player event', t => {
  let pieceStateMix = new PieceState();
  spawnPiece(pieceStateMix, 1, 1);
  spawnPiece(pieceStateMix, 2, 1);
  spawnPiece(pieceStateMix, 14, 1);
  spawnPiece(pieceStateMix, 1, 2);
  spawnPiece(pieceStateMix, 2, 2);

  t.plan(4);
  let queue = new ActionQueue();
  let selector = new Selector(players, pieceStateMix);
  let cardEval = new CardEvaluator(queue, selector, cardDirectory, pieceStateMix);

  cardEval.evaluatePlayerEvent('cardDrawn', 1);

  t.equal(queue._actions.length, 0, 'No actions in the queue yet');

  cardEval.evaluatePlayerEvent('cardDrawn', 2);
  t.equal(queue._actions.length, 1, '1 Action in the queue');
  t.ok(queue._actions[0] instanceof PieceHealthChange, 'First action is Health change');
  t.ok(queue._actions[0].change > 0, 'Heal action');
});
