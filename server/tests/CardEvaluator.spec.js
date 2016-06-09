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
import PieceStatusChange from '../game/ctac/actions/PieceStatusChange.js';
import PieceAttributeChange from '../game/ctac/actions/PieceAttributeChange.js';
import SpawnPiece from '../game/ctac/actions/SpawnPiece.js';
import Player from 'models/Player';
import ActionQueue from 'action-queue';
import CardDirectory from '../game/ctac/models/CardDirectory.js';

//init the dependencies for the evaluator and selector
var cardDirectory = new CardDirectory();

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
  let cardEval = new CardEvaluator(queue, selector, pieceStateMix, mapState);

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
  let cardEval = new CardEvaluator(queue, selector, pieceStateMix, mapState);

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
  let cardEval = new CardEvaluator(queue, selector, pieceStateMix, mapState);

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
  let cardEval = new CardEvaluator(queue, selector, pieceStateMix, mapState);

  let cardPlayed = cardDirectory.directory[16];

  cardEval.evaluateSpellEvent('playSpell', {spellCard: cardPlayed, playerId: 1});

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
  let cardEval = new CardEvaluator(queue, selector, pieceStateMix, mapState);

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
  let cardEval = new CardEvaluator(queue, selector, pieceStateMix, mapState);

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

test('Find Possible spell targets with TechResist', t => {
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

  cardEval.evaluatePieceEvent('playMinion', testBot, 2);

  t.equal(queue._actions.length, 1, '1 Actions in the queue');
  t.ok(queue._actions[0] instanceof PieceStatusChange, 'First action is Piece Status change');
  t.equal(queue._actions[0].add, Statuses.Shield, 'Adding shield status');
});

test('Timers', t => {
  let pieceStateMix = new PieceState();
  spawnPiece(pieceStateMix, 1, 1);
  spawnPiece(pieceStateMix, 2, 1);
  spawnPiece(pieceStateMix, 1, 2);
  spawnPiece(pieceStateMix, 2, 2);

  t.plan(7);
  let queue = new ActionQueue();
  let selector = new Selector(players, pieceStateMix);
  let cardEval = new CardEvaluator(queue, selector, pieceStateMix, mapState);

  let cardPlayed = cardDirectory.newFromId(31);

  cardEval.evaluateSpellEvent('playSpell', {spellCard: cardPlayed, playerId: 1});

  t.equal(queue._actions.length, 1, '1 Actions in the queue');
  t.equal(cardEval.startTurnTimers.length, 1, '1 start turn timer added');

  cardEval.evaluatePlayerEvent('turnStart', 2);
  t.equal(queue._actions.length, 1, 'Still only 1 action in the queue');
  t.equal(cardEval.startTurnTimers.length, 1, 'Still 1 timer saved');

  cardEval.evaluatePlayerEvent('turnStart', 1);
  t.equal(queue._actions.length, 2, 'Triggered another action with timer');
  t.ok(queue._actions[1] instanceof PieceStatusChange, 'New Action coming in is Piece status change');
  t.equal(cardEval.startTurnTimers.length, 0, 'No timers left');
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
  t.ok(queue._actions[0] instanceof PieceAttributeChange, 'First action is Piece Status change');
  t.equal(queue._actions[0].attack, 2, 'Attack is now 2');
  t.ok(queue._actions[1] instanceof PieceAttributeChange, 'Second action is Piece Status change');
  t.equal(queue._actions[1].health, 1, 'Health is now 2');
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

test('Find Possible areas', t => {
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
      cardId: 1,
      event: 'playSpell',
      areaType: 'Cross',
      size: 99,
      isCursor: true,
      isDoubleCursor: false,
      bothDirections: null,
      selfCentered: false,
      centerPosition: null,
      pivotPosition: null,
      areaTiles: []
    },
    {
      cardId: 2,
      event: 'playSpell',
      areaType: 'Line',
      size: 7,
      isCursor: true,
      isDoubleCursor: true,
      bothDirections: false,
      selfCentered: false,
      centerPosition: null,
      pivotPosition: null,
      areaTiles: []
    },
    {
      cardId: 3,
      event: 'playMinion',
      areaType: 'Row',
      size: 3,
      isCursor: true,
      isDoubleCursor: false,
      bothDirections: false,
      selfCentered: true,
      centerPosition: null,
      pivotPosition: null,
      areaTiles: []
    }
  ];

  t.deepEqual(targets, expectedAreas, 'Got back expected areas');


  let otherPlayerTargets = cardEval.findPossibleAreas(cardState.hands[2], 2);
  //expecting area for move
  let otherExpectedTargets = [
    {
      cardId: 4,
      event: 'playMinion',
      areaType: 'Square',
      size: 3,
      isCursor: true,
      isDoubleCursor: false,
      bothDirections: null,
      selfCentered: true,
      centerPosition: null,
      pivotPosition: null,
      areaTiles: []
    }
  ];
  t.deepEqual(otherPlayerTargets, otherExpectedTargets, 'Got back expected areas for move');
});
