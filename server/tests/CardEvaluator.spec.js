import test from 'tape';
import CardEvaluator from '../game/ctac/cardlang/CardEvaluator.js';
import Selector from '../game/ctac/cardlang/Selector.js';
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
import PieceStatusChange from '../game/ctac/actions/PieceStatusChange.js';
import PieceAttributeChange from '../game/ctac/actions/PieceAttributeChange.js';
import PieceBuff from '../game/ctac/actions/PieceBuff.js';
import SpawnPiece from '../game/ctac/actions/SpawnPiece.js';
import Player from 'models/Player';
import ActionQueue from 'action-queue';
import CardDirectory from '../game/ctac/models/CardDirectory.js';

//manually init the dependencies for the evaluator and selector
var cardDirectory = new CardDirectory({cardSets: ['test']});

function spawnPiece(pieceState, cardTemplateId, playerId, addToState = true, position = null){
  var newPiece = pieceState.newFromCard(cardDirectory, cardTemplateId, playerId, null);

  if(addToState){
    pieceState.add(newPiece);
  }
  if(position){
    newPiece.position = position;
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

function queueTypeCheck(t, queue, index, type, message){
  t.equal(typeof queue._actions[index].actionClass, typeof type, message);
}

var player1 = new Player(1);
var player2 = new Player(2);
var players = [player1, player2];

var mapState = new MapState();
mapState.add(cubeland, true);

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
  let cardEval = new CardEvaluator(queue, selector, pieceStateMix, mapState);

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
  let cardEval = new CardEvaluator(queue, selector, pieceStateMix, mapState);

  let testBot = spawnPiece(pieceStateMix, 9, 1, true);
  t.ok(testBot, 'Found writhing bunch');

  cardEval.evaluatePieceEvent('playMinion', testBot);

  t.equal(queue._actions.length, 2, '2 Actions in the queue');
  queueTypeCheck(t, queue, 0, PieceHealthChange, 'First action is Hit');
  queueTypeCheck(t, queue, 1, PieceHealthChange, 'Second action is Hit');
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
  let cardEval = new CardEvaluator(queue, selector, pieceState, mapState);

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
  let cardEval = new CardEvaluator(queue, selector, pieceStateMix, mapState);

  let platypus = spawnPiece(pieceStateMix, 10, 1, true);
  t.ok(platypus, 'Found platypus');

  cardEval.evaluatePieceEvent('playMinion', platypus);

  t.equal(queue._actions.length, 1, '1 Action in the queue');
  queueTypeCheck(t, queue, 0, PieceAttributeChange, 'First action is Set Attribute');
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
  let cardEval = new CardEvaluator(queue, selector, pieceStateMix, mapState);

  let synth = pieceStateMix.pieces.find(p => p.playerId == 1 && p.cardTemplateId == 11);
  t.ok(synth, 'Found synth');

  cardEval.evaluatePieceEvent('damaged', synth);

  t.equal(queue._actions.length, 1, '1 Action in the queue');
  queueTypeCheck(t, queue, 0, PieceHealthChange, 'First action is health change');
  t.equal(queue._actions[0].actionParams.isHit, false, 'Healing not hitting');
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
  let cardEval = new CardEvaluator(queue, selector, pieceStateMix, mapState);

  let spore = pieceStateMix.pieces.find(p => p.playerId == 1 && p.cardTemplateId == 13);
  t.ok(spore, 'Found spore');

  cardEval.evaluatePieceEvent('attacks', spore);

  t.equal(queue._actions.length, 1, '1 Action in the queue');
  queueTypeCheck(t, queue, 0, PieceHealthChange, 'First action is Health change');
  t.equal(queue._actions[0].actionParams.isHit, true, 'Hit action');
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
  let cardEval = new CardEvaluator(queue, selector, pieceStateMix, mapState);

  cardEval.evaluatePlayerEvent('cardDrawn', 1);

  t.equal(queue._actions.length, 0, 'No actions in the queue yet');

  cardEval.evaluatePlayerEvent('cardDrawn', 2);
  t.equal(queue._actions.length, 1, '1 Action in the queue');
  queueTypeCheck(t, queue, 0, PieceHealthChange, 'First action is Health change');
  t.equal(queue._actions[0].actionParams.isHit, false, 'Heal action');
});

test('Spell played event', t => {
  let pieceStateMix = new PieceState();
  spawnPiece(pieceStateMix, 1, 1);
  spawnPiece(pieceStateMix, 2, 1);
  spawnPiece(pieceStateMix, 1, 2);
  spawnPiece(pieceStateMix, 2, 2);

  t.plan(4);
  let queue = new ActionQueue();
  let selector = new Selector(players, pieceStateMix);
  let cardEval = new CardEvaluator(queue, selector, pieceStateMix, mapState);

  let cardPlayed = cardDirectory.directory[16];

  cardEval.evaluateSpellEvent('playSpell', {spellCard: cardPlayed, playerId: 1});

  t.equal(queue._actions.length, 1, '1 Actions in the queue');
  queueTypeCheck(t, queue, 0, PieceHealthChange, 'First action is Health change');
  t.equal(queue._actions[0].actionParams.isHit, true, 'Hit action');
  t.ok(queue._actions[0].selector, 'Has Piece Selector');
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
  let cardEval = new CardEvaluator(queue, selector, pieceStateMix, mapState);

  let testBot = spawnPiece(pieceStateMix, 17, 1, true);

  cardEval.evaluatePieceEvent('playMinion', testBot, {targetPieceId: 4});

  t.equal(queue._actions.length, 1, '1 Actions in the queue');
  queueTypeCheck(t, queue, 0, PieceHealthChange, 'First action is Health change');
  t.equal(queue._actions[0].actionParams.isHit, true, 'Hit action');
  t.equal(queue._actions[0].pieceSelectorParams.targetPieceId, 4, 'Targeted the right piece');
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
  let cardEval = new CardEvaluator(queue, selector, pieceStateMix, mapState);

  let testBot = spawnPiece(pieceStateMix, 19, 1, true);

  //the syndicate requires a friendly minion, this is not so we should get a message back
  let evalReturn = cardEval.evaluatePieceEvent('playMinion', testBot, {targetPieceId: 3});

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
  let cardEval = new CardEvaluator(queue, selector, pieceStateMix, mapState);

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

test('Find Possible targets for timer', t => {
  let pieceStateMix = new PieceState();
  spawnPiece(pieceStateMix, 1, 1); //id 1
  spawnPiece(pieceStateMix, 2, 1); //id 2
  spawnPiece(pieceStateMix, 1, 2); //id 3
  spawnPiece(pieceStateMix, 2, 2); //id 4

  let cardState = new CardState();
  cardState.initPlayer(1);
  spawnCard(cardState, 1, 65);

  t.plan(1);
  let queue = new ActionQueue();
  let selector = new Selector(players, pieceStateMix);
  let cardEval = new CardEvaluator(queue, selector, pieceStateMix, mapState);

  let targets = cardEval.findPossibleTargets(cardState.hands[1], 1);
  //expecting enemy minions
  let expectedTargets = [
    {cardId: 1, event: 'playSpell', targetPieceIds: [4]}
  ];

  t.deepEqual(targets, expectedTargets, 'Got targets for timer action');
});

test('Find Possible spell targets with Elusive', t => {
  let pieceStateMix = new PieceState();
  spawnPiece(pieceStateMix, 1, 1); //id 1
  spawnPiece(pieceStateMix, 2, 1); //id 2
  spawnPiece(pieceStateMix, 35, 1); //id 3
  spawnPiece(pieceStateMix, 1, 2); //id 4
  spawnPiece(pieceStateMix, 2, 2); //id 5

  //fill one player hands with a target card and some other random ones to try to catch other errors
  let cardState = new CardState();
  cardState.initPlayer(1);
  cardState.initPlayer(2);
  spawnCard(cardState, 1, 18); //id 1
  spawnCard(cardState, 1, 22); //id 2
  spawnCard(cardState, 2, 3); //id 3
  spawnCard(cardState, 2, 4); //id 4

  t.plan(1);
  let queue = new ActionQueue();
  let selector = new Selector(players, pieceStateMix);
  let cardEval = new CardEvaluator(queue, selector, pieceStateMix, mapState);

  let targets = cardEval.findPossibleTargets(cardState.hands[1], 1);
  //expecting enemy characters
  let expectedTargets = [
    {cardId: 1, event: 'playMinion', targetPieceIds: [2, 3, 5]},
    {cardId: 2, event: 'playSpell', targetPieceIds: [2]}
  ];

  t.deepEqual(targets, expectedTargets, 'Player 1 can target with minion but not with spell');

});

test('Find Possible targets for eventual number', t => {
  let pieceStateMix = new PieceState();
  spawnPiece(pieceStateMix, 1, 1); //id 1
  spawnPiece(pieceStateMix, 2, 1); //id 2
  spawnPiece(pieceStateMix, 1, 2); //id 3
  spawnPiece(pieceStateMix, 2, 2); //id 4

  let cardState = new CardState();
  cardState.initPlayer(1);
  spawnCard(cardState, 1, 71);

  t.plan(1);
  let queue = new ActionQueue();
  let selector = new Selector(players, pieceStateMix);
  let cardEval = new CardEvaluator(queue, selector, pieceStateMix, mapState);

  let targets = cardEval.findPossibleTargets(cardState.hands[1], 1);
  //expecting friendly minions
  let expectedTargets = [
    {cardId: 1, event: 'playSpell', targetPieceIds: [2]}
  ];

  t.deepEqual(targets, expectedTargets, 'Got targets for eventual number');
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
  let cardEval = new CardEvaluator(queue, selector, pieceStateMix, mapState);

  let testBot = spawnPiece(pieceStateMix, 25, 1, false);
  testBot.position = new Position(1, 0, 1);

  cardEval.evaluatePieceEvent('death', testBot);

  t.equal(queue._actions.length, 1, '1 Actions in the queue');
  t.ok(queue._actions[0] instanceof SpawnPiece, 'First action is Spawn Piece');
});

test('Statuses', t => {
  let pieceStateMix = new PieceState();
  spawnPiece(pieceStateMix, 1, 1);
  spawnPiece(pieceStateMix, 2, 1);
  spawnPiece(pieceStateMix, 1, 2);
  spawnPiece(pieceStateMix, 2, 2);

  let testBot = spawnPiece(pieceStateMix, 28, 1);
  t.plan(1);

  t.equal(testBot.statuses, Statuses.Shield, 'Piece has shield status');
});

test('Give Status', t => {
  let pieceStateMix = new PieceState();
  spawnPiece(pieceStateMix, 1, 1);
  spawnPiece(pieceStateMix, 2, 1);
  spawnPiece(pieceStateMix, 1, 2);
  spawnPiece(pieceStateMix, 2, 2);

  t.plan(3);
  let queue = new ActionQueue();
  let selector = new Selector(players, pieceStateMix);
  let cardEval = new CardEvaluator(queue, selector, pieceStateMix, mapState);

  let testBot = spawnPiece(pieceStateMix, 29, 1);

  cardEval.evaluatePieceEvent('playMinion', testBot, {targetPieceId: 2});

  t.equal(queue._actions.length, 1, '1 Actions in the queue');
  queueTypeCheck(t, queue, 0, PieceStatusChange, 'First action is Piece Status change');
  t.equal(queue._actions[0].actionParams.add, Statuses.Shield, 'Adding shield status');
});

test('Swap attack and health', t => {
  let pieceStateMix = new PieceState();
  spawnPiece(pieceStateMix, 1, 1);
  spawnPiece(pieceStateMix, 2, 1);
  spawnPiece(pieceStateMix, 1, 2);
  spawnPiece(pieceStateMix, 2, 2); //id 4

  t.plan(5);
  let queue = new ActionQueue();
  let selector = new Selector(players, pieceStateMix);
  let cardEval = new CardEvaluator(queue, selector, pieceStateMix, mapState);

  let cardPlayed = cardDirectory.directory[40];

  cardEval.evaluateSpellEvent('playSpell', {spellCard: cardPlayed, playerId: 1, targetPieceId: 4});

  t.equal(queue._actions.length, 2, '2 Actions in the queue');
  queueTypeCheck(t, queue, 0, PieceAttributeChange, 'First action is Piece Status change');
  t.equal(queue._actions[0].actionParams.attack, 2, 'Attack is now 2');
  queueTypeCheck(t, queue, 1, PieceAttributeChange, 'Second action is Piece Status change');
  t.equal(queue._actions[1].actionParams.health, 1, 'Health is now 2');
});

test('Find Possible abilities', t => {
  let pieceStateMix = new PieceState();
  spawnPiece(pieceStateMix, 1, 1); //id 1
  spawnPiece(pieceStateMix, 2, 1); //id 2
  spawnPiece(pieceStateMix, 1, 2); //id 3
  spawnPiece(pieceStateMix, 2, 2); //id 4

  t.plan(1);
  let queue = new ActionQueue();
  let selector = new Selector(players, pieceStateMix);
  let cardEval = new CardEvaluator(queue, selector, pieceStateMix, mapState);

  let targets = cardEval.findPossibleAbilities(pieceStateMix.pieces, 1);
  //all pieces
  let expectedTargets = [
    {
      pieceId: 1,
      abilityCost: 2,
      abilityChargeTime: 1,
      abilityCooldown: 1,
      ability: 'Overwatch Shot',
      targetPieceIds: [1, 2, 3, 4]
    }
  ];

  t.deepEqual(targets, expectedTargets, 'Player 1 ability targets is everyone');
});

test('Find Possible Card areas', t => {
  let pieceStateMix = new PieceState();
  spawnPiece(pieceStateMix, 1, 1); //id 1
  spawnPiece(pieceStateMix, 2, 1); //id 2
  spawnPiece(pieceStateMix, 1, 2); //id 3
  spawnPiece(pieceStateMix, 2, 2); //id 4

  //fill one player hands with some area cards
  let cardState = new CardState();
  cardState.initPlayer(1);
  cardState.initPlayer(2);
  spawnCard(cardState, 1, 49); //id 1
  spawnCard(cardState, 1, 51);
  spawnCard(cardState, 1, 52);
  spawnCard(cardState, 2, 63); //id 4

  t.plan(2);
  let queue = new ActionQueue();
  let selector = new Selector(players, pieceStateMix);
  let cardEval = new CardEvaluator(queue, selector, pieceStateMix, mapState);

  let targets = cardEval.findPossibleAreas(cardState.hands[1], 1);

  let expectedAreas = [
    {
      cardOrPieceId: 1,
      event: 'playSpell',
      areaType: 'Cross',
      size: 99,
      isCursor: true,
      isDoubleCursor: false,
      bothDirections: null,
      selfCentered: false,
      stationaryArea: false,
      centerPosition: null,
      pivotPosition: null,
      moveRestricted: false,
      areaTiles: []
    },
    {
      cardOrPieceId: 2,
      event: 'playSpell',
      areaType: 'Line',
      size: 7,
      isCursor: true,
      isDoubleCursor: true,
      bothDirections: false,
      selfCentered: false,
      stationaryArea: false,
      centerPosition: null,
      pivotPosition: null,
      moveRestricted: false,
      areaTiles: []
    },
    {
      cardOrPieceId: 3,
      event: 'playMinion',
      areaType: 'Row',
      size: 3,
      isCursor: true,
      isDoubleCursor: false,
      bothDirections: false,
      selfCentered: true,
      stationaryArea: false,
      centerPosition: null,
      pivotPosition: null,
      moveRestricted: false,
      areaTiles: []
    }
  ];

  t.deepEqual(targets, expectedAreas, 'Got back expected areas');


  let otherPlayerTargets = cardEval.findPossibleAreas(cardState.hands[2], 2);
  //expecting area for move
  let otherExpectedTargets = [
    {
      cardOrPieceId: 4,
      event: 'playMinion',
      areaType: 'Square',
      size: 3,
      isCursor: true,
      isDoubleCursor: false,
      bothDirections: null,
      selfCentered: true,
      stationaryArea: true,
      centerPosition: null,
      pivotPosition: null,
      moveRestricted: true,
      areaTiles: []
    }
  ];
  t.deepEqual(otherPlayerTargets, otherExpectedTargets, 'Got back expected areas for move');
});

test.only('Find Possible Piece Areas', t => {
  let pieceStateMix = new PieceState();
  spawnPiece(pieceStateMix, 1, 1, true, new Position(0, 0, 0));
  spawnPiece(pieceStateMix, 2, 1, true, new Position(0, 0, 1));
  spawnPiece(pieceStateMix, 1, 2, true, new Position(2, 0, 2));
  spawnPiece(pieceStateMix, 2, 2, true, new Position(2, 0, 1));

  t.plan(1);
  let queue = new ActionQueue();
  let selector = new Selector(players, pieceStateMix, mapState);
  let cardEval = new CardEvaluator(queue, selector, pieceStateMix, mapState);

  let testBot = spawnPiece(pieceStateMix, 116, 1);
  testBot.position = new Position(1, 0, 1);

  cardEval.evaluatePieceEvent('playMinion', testBot);

  let targets = cardEval.findPieceAreas(pieceStateMix.pieces.filter(p => p.playerId === 1), 1);

  let expectedAreas = [
    {
      cardOrPieceId: testBot.id,
      event: 'endTurnTimer',
      areaType: 'Cross',
      size: 4,
      isCursor: false,
      isDoubleCursor: false,
      bothDirections: true,
      selfCentered: true,
      stationaryArea: false,
      centerPosition: null,
      pivotPosition: null,
      moveRestricted: false,
      areaTiles: []
    },
    {
      cardOrPieceId: testBot.id,
      event: 'endTurnTimer',
      areaType: 'Diagonal',
      size: 4,
      isCursor: false,
      isDoubleCursor: false,
      bothDirections: true,
      selfCentered: true,
      stationaryArea: false,
      centerPosition: null,
      pivotPosition: null,
      moveRestricted: false,
      areaTiles: []
    },
    {
      cardOrPieceId: testBot.id,
      event: 'endTurnTimer',
      areaType: 'Diagonal',
      size: 4,
      isCursor: false,
      isDoubleCursor: false,
      bothDirections: true,
      selfCentered: true,
      stationaryArea: false,
      centerPosition: null,
      pivotPosition: null,
      moveRestricted: false,
      areaTiles: []
    }
  ];

  t.deepEqual(targets, expectedAreas, 'Got back expected areas');
});

test('Conditional Action', t => {
  let pieceStateMix = new PieceState();
  spawnPiece(pieceStateMix, 1, 1);
  spawnPiece(pieceStateMix, 2, 1);
  spawnPiece(pieceStateMix, 1, 2);
  spawnPiece(pieceStateMix, 2, 2);

  t.plan(3);
  let queue = new ActionQueue();
  let selector = new Selector(players, pieceStateMix);
  let cardEval = new CardEvaluator(queue, selector, pieceStateMix, mapState);

  let testBot = spawnPiece(pieceStateMix, 72, 1);

  cardEval.evaluatePieceEvent('playMinion', testBot, {targetPieceId: 1});

  t.equal(queue._actions.length, 1, '1 Actions in the queue');
  queueTypeCheck(t, queue, 0, PieceBuff, 'First action is Piece Buff');

  //now without any minions shouldn't get the buff
  let sparsePieceState = new PieceState();
  spawnPiece(sparsePieceState, 1, 1);
  spawnPiece(sparsePieceState, 1, 2);

  queue = new ActionQueue();
  selector = new Selector(players, sparsePieceState);
  cardEval = new CardEvaluator(queue, selector, sparsePieceState, mapState);

  testBot = spawnPiece(pieceStateMix, 72, 1);

  cardEval.evaluatePieceEvent('playMinion', testBot, {targetPieceId: 1});

  t.equal(queue._actions.length, 0, '0 Actions in the queue for unmet condition');
});

test('Find condition met cards', t => {
  let pieceStateMix = new PieceState();
  spawnPiece(pieceStateMix, 1, 1); //id 1
  spawnPiece(pieceStateMix, 2, 1); //id 2
  spawnPiece(pieceStateMix, 1, 2); //id 3
  spawnPiece(pieceStateMix, 2, 2); //id 4

  //fill one player hands with some condition cards
  let cardState = new CardState();
  cardState.initPlayer(1);
  spawnCard(cardState, 1, 49); //id 1
  spawnCard(cardState, 1, 72); //id 2

  t.plan(2);
  let queue = new ActionQueue();
  let selector = new Selector(players, pieceStateMix);
  let cardEval = new CardEvaluator(queue, selector, pieceStateMix, mapState);

  let conditionals = cardEval.findMetConditionCards(cardState.hands[1], 1);
  let expectedConditionals = [
    {
      cardId: 2,
    }
  ];

  t.deepEqual(conditionals, expectedConditionals, 'Card 2 met the activate condition');

  let noPieceState = new PieceState();
  let emptySelector = new Selector(players, noPieceState);
  let emptyCardEval = new CardEvaluator(queue, emptySelector, noPieceState, mapState);

  let noConditionals = emptyCardEval.findMetConditionCards(cardState.hands[1], 1);
  t.deepEqual(noConditionals, [], 'Nothing met activate criteria');
});

test('Find choose cards', {objectPrintDepth: 9}, t => {
  let pieceStateMix = new PieceState();
  spawnPiece(pieceStateMix, 1, 1); //id 1
  spawnPiece(pieceStateMix, 2, 1); //id 2
  spawnPiece(pieceStateMix, 1, 2); //id 3
  spawnPiece(pieceStateMix, 2, 2); //id 4

  //fill one player hands with a target card and some other random ones to try to catch other errors
  let cardState = new CardState();
  cardState.initPlayer(1);
  cardState.initPlayer(2);
  spawnCard(cardState, 1, 81); //id 1
  spawnCard(cardState, 1, 3);
  spawnCard(cardState, 2, 8); //id 6
  spawnCard(cardState, 2, 9); //id 7

  t.plan(1);
  let queue = new ActionQueue();
  let selector = new Selector(players, pieceStateMix);
  let cardEval = new CardEvaluator(queue, selector, pieceStateMix, mapState);

  let choices = cardEval.findChooseCards(cardState.hands[1], 1, cardDirectory);
  //expecting enemy characters
  let expectedChoices = [
    {
      cardId: 1,
      choices: [
        {cardTemplateId: 82, targets: null},
        {cardTemplateId: 83, targets: {cardId: null, event: 'playSpell', targetPieceIds: [2, 4]}}
      ]
    }
  ];

  t.deepEqual(choices, expectedChoices, 'Player 1 choices are enemy characters');
});

test('Play a teleport off the map', t => {
  let pieceStateMix = new PieceState();
  let hero1 = spawnPiece(pieceStateMix, 1, 1);
  let testSubject = spawnPiece(pieceStateMix, 2, 1);
  let hero2 = spawnPiece(pieceStateMix, 1, 2);

  t.plan(1);
  let queue = new ActionQueue();
  let selector = new Selector(players, pieceStateMix, mapState);
  let cardEval = new CardEvaluator(queue, selector, pieceStateMix, mapState);

  let cardPlayed = cardDirectory.directory[64];

  testSubject.position = new Position(4, 0, 4);
  hero1.position = new Position(1, 0, 1);
  hero2.position = new Position(1, 0, 2);

  let spellEvalResult = cardEval.evaluateSpellEvent('playSpell', {
    spellCard: cardPlayed,
    playerId: 1,
    targetPieceId: testSubject.id,
    pivotPosition: new Position(4, 0, 5)
  });

  t.equal(spellEvalResult, false, 'Rejected spell event');
});
