import PassTurn from '../actions/PassTurn.js';
import DrawCard from '../actions/DrawCard.js';

/**
 * Handle the PassTurn action
 */
export default class TurnProcessor
{
  constructor(turnState, players, playerResourceState)
  {
    this.turnState = turnState;
    this.players = players;
    this.playerResourceState = playerResourceState;
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
        queue.cancel(action);
        return;
      }
    }

    // do it
    var currentTurn = this.turnState.passTurnTo(action.to);

    //give some handouts
    action.toPlayerResources = this.playerResourceState.incriment(action.to, currentTurn);
    action.currentTurn = currentTurn;

    queue.push(new DrawCard(action.to));
    queue.complete(action);
  }
}
