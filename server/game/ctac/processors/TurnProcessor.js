import PassTurn from '../actions/PassTurn.js';
import DrawCard from '../actions/DrawCard.js';
import PieceStatusChange from '../actions/PieceStatusChange.js';
import Statuses from '../models/Statuses.js';

/**
 * Handle the PassTurn action
 */
export default class TurnProcessor
{
  constructor(turnState, players, playerResourceState, cardEvaluator, selector)
  {
    this.turnState = turnState;
    this.players = players;
    this.playerResourceState = playerResourceState;
    this.cardEvaluator = cardEvaluator;
    this.selector = selector;
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

    this.cardEvaluator.evaluatePlayerEvent('turnEnd', action.from);

    // do it
    var currentTurn = this.turnState.passTurnTo(action.to);

    //give some handouts
    action.toPlayerResources = this.playerResourceState.incriment(action.to, currentTurn);
    action.currentTurn = currentTurn;

    //clear statuses for pieces that have had them for a turn
    //doesn't actually wait for a turn right now though
    let select =
    {
      left: 'PARALYZE',
      op: '&',
      right: 'FRIENDLY'
    };
    let paralyzed = this.selector.selectPieces(action.to, select);
    if(paralyzed.length > 0){
      for(let s of paralyzed){
        queue.push(new PieceStatusChange(s.id, null, Statuses.Paralyze ));
      }
    }

    //and finally eval the new turn
    this.cardEvaluator.evaluatePlayerEvent('turnStart', action.to);

    queue.push(new DrawCard(action.to));
    queue.complete(action);
  }
}
