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

var player1 = new Player(1);
var player2 = new Player(2);
var players = [player1, player2];

//various setups for game state
var pieceStateMix = new PieceState();
spawnPiece(pieceStateMix, 1, 1);
spawnPiece(pieceStateMix, 2, 1);
spawnPiece(pieceStateMix, 3, 1);
spawnPiece(pieceStateMix, 9, 1);
spawnPiece(pieceStateMix, 1, 2);
spawnPiece(pieceStateMix, 2, 2);
spawnPiece(pieceStateMix, 3, 2);


test('Basic Draw card', t => {
  t.plan(4);
  let queue = new ActionQueue();
  let selector = new Selector(players, pieceStateMix);
  let cardEval = new CardEvaluator(queue, selector, cardDirectory);

  let testBot = pieceStateMix.pieces.filter(p => p.playerId == 1 && p.cardId == 3)[0]; 
  t.ok(testBot, 'Found test bot');

  cardEval.evaluateAction('play', testBot);

  t.equal(queue._actions.length, 2, '2 Actions in the queue');
  t.ok(queue._actions[0] instanceof DrawCard, 'First action is Draw Card');
  t.ok(queue._actions[1] instanceof DrawCard, 'Second action is Draw Card');
});

test('Basic Hit action', t => {
  t.plan(4);
  let queue = new ActionQueue();
  let selector = new Selector(players, pieceStateMix);
  let cardEval = new CardEvaluator(queue, selector, cardDirectory);

  let testBot = pieceStateMix.pieces.filter(p => p.playerId == 1 && p.cardId == 9)[0]; 
  t.ok(testBot, 'Found writhing bunch');

  cardEval.evaluateAction('play', testBot);

  t.equal(queue._actions.length, 2, '2 Actions in the queue');
  t.ok(queue._actions[0] instanceof PieceHealthChange, 'First action is Hit');
  t.ok(queue._actions[1] instanceof PieceHealthChange, 'Second action is Hit');
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
