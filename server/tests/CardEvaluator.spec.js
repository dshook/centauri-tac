import test from 'tape';
import CardEvaluator from '../game/ctac/cardlang/CardEvaluator.js';
import Selector from '../game/ctac/cardlang/Selector.js';
import GamePiece from '../game/ctac/models/GamePiece.js';
import PieceState from '../game/ctac/models/PieceState.js';
import CardState from '../game/ctac/models/CardState.js';
import Card from '../game/ctac/models/Card.js';
import Position from '../game/ctac/models/Position.js';
import MapState from '../game/ctac/models/MapState.js';
import Statuses from '../game/ctac/models/Statuses.js';
import cubeland from '../../maps/cubeland.json';
import DrawCard from '../game/ctac/actions/DrawCard.js';
import Message from '../game/ctac/actions/Message.js';
import PieceHealthChange from '../game/ctac/actions/PieceHealthChange.js';
import PieceAttributeChange from '../game/ctac/actions/PieceAttributeChange.js';
import SpawnPiece from '../game/ctac/actions/SpawnPiece.js';
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

function spawnPiece(pieceState, cardTemplateId, playerId, addToState = true){
    var newPiece = pieceState.newFromCard(cardDirectory, cardTemplateId, playerId, null);

    if(addToState){
      pieceState.add(newPiece);
    }
    return newPiece;
}

function spawnCard(cardState, playerId, cardTemplateId){
  let directoryCard = cardDirectory.directory[cardTemplateId];
  //clone into new card
  var cardClone = new Card();
  for(var k in directoryCard) cardClone[k] = directoryCard[k];

  //add to deck the immediately draw so it's in hand
  cardState.addToDeck(playerId, cardClone);
  cardState.drawCard(playerId);
}

var player1 = new Player(1);
var player2 = new Player(2);
var players = [player1, player2];

var mapState = new MapState();
mapState.add(cubeland);

test('Basic Draw card', t => {
  let pieceStateMix = new PieceState();
  spawnPiece(pieceStateMix, 1, 1);
  spawnPiece(pieceStateMix, 2, 1);
  spawnPiece(pieceStateMix, 9, 1);
  spawnPiece(pieceStateMix, 1, 2);
  spawnPiece(pieceStateMix, 2, 2);
  spawnPiece(pieceStateMix, 3, 2);

  t.plan(4);
  let queue = new ActionQueue();
  let selector = new Selector(players, pieceStateMix);
  let cardEval = new CardEvaluator(queue, selector, cardDirectory, pieceStateMix, mapState);

  let testBot = spawnPiece(pieceStateMix, 3, 1, false);
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
  spawnPiece(pieceStateMix, 1, 2);
  spawnPiece(pieceStateMix, 2, 2);
  spawnPiece(pieceStateMix, 3, 2);

  t.plan(4);
  let queue = new ActionQueue();
  let selector = new Selector(players, pieceStateMix);
  let cardEval = new CardEvaluator(queue, selector, cardDirectory, pieceStateMix, mapState);

  let testBot = spawnPiece(pieceStateMix, 9, 1, true);
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
  let cardEval = new CardEvaluator(queue, selector, cardDirectory, pieceState, mapState);

  let friendlyHero = pieceState.pieces.find(p => p.playerId == 1 && p.cardTemplateId == 1);
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
  spawnPiece(pieceStateMix, 1, 2);
  spawnPiece(pieceStateMix, 2, 2);
  spawnPiece(pieceStateMix, 3, 2);

  t.plan(3);
  let queue = new ActionQueue();
  let selector = new Selector(players, pieceStateMix);
  let cardEval = new CardEvaluator(queue, selector, cardDirectory, pieceStateMix, mapState);

  let platypus = spawnPiece(pieceStateMix, 10, 1, true);
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
  let cardEval = new CardEvaluator(queue, selector, cardDirectory, pieceStateMix, mapState);

  let synth = pieceStateMix.pieces.find(p => p.playerId == 1 && p.cardTemplateId == 11);
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
  let cardEval = new CardEvaluator(queue, selector, cardDirectory, pieceStateMix, mapState);

  let spore = pieceStateMix.pieces.find(p => p.playerId == 1 && p.cardTemplateId == 13);
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

  //damage a friendly unit so there's something to heal
  let friendlyHero = pieceStateMix.pieces.find(p => p.playerId == 1 && p.cardTemplateId == 1);
  friendlyHero.health -= 5;

  t.plan(4);
  let queue = new ActionQueue();
  let selector = new Selector(players, pieceStateMix);
  let cardEval = new CardEvaluator(queue, selector, cardDirectory, pieceStateMix, mapState);

  cardEval.evaluatePlayerEvent('cardDrawn', 1);

  t.equal(queue._actions.length, 0, 'No actions in the queue yet');

  cardEval.evaluatePlayerEvent('cardDrawn', 2);
  t.equal(queue._actions.length, 1, '1 Action in the queue');
  t.ok(queue._actions[0] instanceof PieceHealthChange, 'First action is Health change');
  t.ok(queue._actions[0].change > 0, 'Heal action');
});

test('Spell played event', t => {
  let pieceStateMix = new PieceState();
  spawnPiece(pieceStateMix, 1, 1);
  spawnPiece(pieceStateMix, 2, 1);
  spawnPiece(pieceStateMix, 1, 2);
  spawnPiece(pieceStateMix, 2, 2);

  t.plan(3);
  let queue = new ActionQueue();
  let selector = new Selector(players, pieceStateMix);
  let cardEval = new CardEvaluator(queue, selector, cardDirectory, pieceStateMix, mapState);

  let cardPlayed = cardDirectory.directory[16];

  cardEval.evaluateSpellEvent('playSpell', cardPlayed, 1);

  t.equal(queue._actions.length, 4, '4 Actions in the queue');
  t.ok(queue._actions[0] instanceof PieceHealthChange, 'First action is Health change');
  t.ok(queue._actions[0].change < 0, 'Hit action');
});

test('Targeting minions', t => {
  let pieceStateMix = new PieceState();
  spawnPiece(pieceStateMix, 1, 1); //id 1
  spawnPiece(pieceStateMix, 2, 1); //id 2
  spawnPiece(pieceStateMix, 1, 2); //id 3
  spawnPiece(pieceStateMix, 2, 2); //id 4

  t.plan(4);
  let queue = new ActionQueue();
  let selector = new Selector(players, pieceStateMix);
  let cardEval = new CardEvaluator(queue, selector, cardDirectory, pieceStateMix, mapState);

  let testBot = spawnPiece(pieceStateMix, 17, 1, true);

  cardEval.evaluatePieceEvent('playMinion', testBot, 4);

  t.equal(queue._actions.length, 1, '1 Actions in the queue');
  t.ok(queue._actions[0] instanceof PieceHealthChange, 'First action is Health change');
  t.ok(queue._actions[0].change < 0, 'Hit action');
  t.equal(queue._actions[0].pieceId, 4, 'Targeted the right piece');
});

test('Targeting invalid', t => {
  let pieceStateMix = new PieceState();
  spawnPiece(pieceStateMix, 1, 1); //id 1
  spawnPiece(pieceStateMix, 2, 1); //id 2
  spawnPiece(pieceStateMix, 1, 2); //id 3
  spawnPiece(pieceStateMix, 2, 2); //id 4

  t.plan(3);
  let queue = new ActionQueue();
  let selector = new Selector(players, pieceStateMix);
  let cardEval = new CardEvaluator(queue, selector, cardDirectory, pieceStateMix, mapState);

  let testBot = spawnPiece(pieceStateMix, 19, 1, true);

  //the syndicate requires a friendly minion, this is not so we should get a message back
  let evalReturn = cardEval.evaluatePieceEvent('playMinion', testBot, 3);

  t.equal(evalReturn, false, 'Eval returned false for invalid target');
  t.equal(queue._actions.length, 1, '1 Actions in the queue');
  t.ok(queue._actions[0] instanceof Message, 'First action is Message');
});

test('Find Possible targets', t => {
  let pieceStateMix = new PieceState();
  spawnPiece(pieceStateMix, 1, 1); //id 1
  spawnPiece(pieceStateMix, 2, 1); //id 2
  spawnPiece(pieceStateMix, 1, 2); //id 3
  spawnPiece(pieceStateMix, 2, 2); //id 4

  //fill one player hands with a target card and some other random ones to try to catch other errors
  let cardState = new CardState();
  cardState.initPlayer(1);
  cardState.initPlayer(2);
  spawnCard(cardState, 1, 17); //id 1
  spawnCard(cardState, 1, 3);
  spawnCard(cardState, 1, 4);
  spawnCard(cardState, 1, 5);
  spawnCard(cardState, 1, 6);
  spawnCard(cardState, 2, 18); //id 6
  spawnCard(cardState, 2, 18); //id 7

  t.plan(2);
  let queue = new ActionQueue();
  let selector = new Selector(players, pieceStateMix);
  let cardEval = new CardEvaluator(queue, selector, cardDirectory, pieceStateMix, mapState);

  let targets = cardEval.findPossibleTargets(cardState.hands[1], 1);
  //expecting enemy characters
  let expectedTargets = [
    {cardId: 1, event: 'playMinion', targetPieceIds: [3, 4]}
  ];

  t.deepEqual(targets, expectedTargets, 'Player 1 targets are enemy characters');


  let otherPlayerTargets = cardEval.findPossibleTargets(cardState.hands[2], 2);
  //expecting all minions
  let otherExpectedTargets = [
    {cardId: 6, event: 'playMinion', targetPieceIds: [2, 4]},
    {cardId: 7, event: 'playMinion', targetPieceIds: [2, 4]}
  ];
  t.deepEqual(otherPlayerTargets, otherExpectedTargets, 'Player 2 targets are minions');
});

test('Spawn a piece', t => {
  let pieceStateMix = new PieceState();
  spawnPiece(pieceStateMix, 1, 1);
  spawnPiece(pieceStateMix, 2, 1);
  spawnPiece(pieceStateMix, 9, 1);
  spawnPiece(pieceStateMix, 1, 2);
  spawnPiece(pieceStateMix, 2, 2);
  spawnPiece(pieceStateMix, 3, 2);

  t.plan(2);
  let queue = new ActionQueue();
  let selector = new Selector(players, pieceStateMix);
  let cardEval = new CardEvaluator(queue, selector, cardDirectory, pieceStateMix, mapState);

  let testBot = spawnPiece(pieceStateMix, 25, 1, false);
  testBot.position = new Position(1,0,1);

  cardEval.evaluatePieceEvent('death', testBot);

  t.equal(queue._actions.length, 1, '1 Actions in the queue');
  t.ok(queue._actions[0] instanceof SpawnPiece, 'First action is Draw Card');
});

test('Statuses', t => {
  let pieceStateMix = new PieceState();
  spawnPiece(pieceStateMix, 1, 1);
  spawnPiece(pieceStateMix, 2, 1);
  spawnPiece(pieceStateMix, 1, 2);
  spawnPiece(pieceStateMix, 2, 2);

  let testBot = spawnPiece(pieceStateMix, 28, 1);
  t.plan(1);

  t.equal(testBot.statuses[0], Statuses.shield, 'Piece has shield status');
});
