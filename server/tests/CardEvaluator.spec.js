import test from 'tape';
import CardEvaluator from '../game/ctac/cardlang/CardEvaluator.js';
import Selector from '../game/ctac/cardlang/Selector.js';
import TurnState from '../game/ctac/models/TurnState.js';
import PieceState from '../game/ctac/models/PieceState.js';
import Player from 'models/Player';
import ActionQueue from 'action-queue';
import requireDir from 'require-dir';
import CardDirectory from '../game/ctac/models/CardDirectory.js';

//init the dependencies for the evaluator and selector
const queue = new ActionQueue();

var cardRequires = requireDir('../../cards/');
var cardDirectory = new CardDirectory();

for(let cardFileName in cardRequires){
  let card = cardRequires[cardFileName];
  cardDirectory.add(card);
}

var turnState = new TurnState();

var player1 = new Player(1);
var player2 = new Player(2);
var players = [player1, player2];

var pieceState = new PieceState();

var selector = new Selector(turnState, players, pieceState);

var cardEval = new CardEvaluator(queue, selector, cardDirectory);

test('init', t => {
  t.plan(2);
  t.notEqual(selector, null, 'Built a selector');
  t.notEqual(cardEval, null, 'Built card eval');

});