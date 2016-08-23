import PassTurn from '../actions/PassTurn.js';
import DrawCard from '../actions/DrawCard.js';

/**
 * Handle the PassTurn action
 */
export default class TurnProcessor
{
  constructor(turnState, players, playerResourceState, cardEvaluator, pieceState, statsState)
  {
    this.turnState = turnState;
    this.players = players;
    this.playerResourceState = playerResourceState;
    this.cardEvaluator = cardEvaluator;
    this.pieceState = pieceState;
    this.statsState = statsState;
  }

  /**
   * Proc
   */
  async handleAction(action, queue)
  {
    if (!(action instanceof PassTurn)) {
      return;
    }

    // only have to validate if its from a player
    if (action.from) {
      const fromOkay = this.turnState.currentPlayerId === action.from;
      const toOkay = this.players.some(x => x.id === action.to);

      if (!fromOkay || !toOkay) {
        return queue.cancel(action);
      }
    }

    //incriment piece ability charges and reset counter
    let playerPieces = this.pieceState.pieces.filter(x => x.playerId === action.to);
    for(let piece of playerPieces){
      piece.abilityCharge++;
      piece.hasMoved = false;
      piece.attackCount = 0;
    }

    this.cardEvaluator.evaluatePlayerEvent('turnEnd', action.from);

    // do it
    var currentTurn = this.turnState.passTurnTo(action.to);

    //give some handouts
    action.toPlayerMaxResources = this.playerResourceState.incriment(action.to, 1);
    action.toPlayerResources = this.playerResourceState.refill(action.to);
    action.currentTurn = currentTurn;

    //update stats
    this.statsState.stats['COMBOCOUNT'] = 0;

    //and finally eval the new turn
    this.cardEvaluator.evaluatePlayerEvent('turnStart', action.to);

    queue.push(new DrawCard(action.to));
    queue.complete(action);
  }
}
