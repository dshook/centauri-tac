import loglevel from 'loglevel-decorator';
import PassTurn from '../actions/PassTurn.js';
import DrawCard from '../actions/DrawCard.js';

/**
 * Handle the PassTurn action
 */
@loglevel
export default class TurnProcessor
{
  constructor(turnState, players, playerResourceState, cardEvaluator, pieceState, statsState, gameEventService)
  {
    this.turnState = turnState;
    this.players = players;
    this.playerResourceState = playerResourceState;
    this.cardEvaluator = cardEvaluator;
    this.pieceState = pieceState;
    this.statsState = statsState;
    this.gameEventService = gameEventService;
  }

  /**
   * Proc
   */
  async handleAction(action, queue)
  {
    if (!(action instanceof PassTurn)) {
      return;
    }

    //incriment piece ability charges and reset counter
    for(let piece of this.pieceState.pieces){
      piece.abilityCharge++;
      piece.hasMoved = false;
      piece.attackCount = 0;
    }

    //@TODO: come back and see if there's a better way to eval the turnEnd and turnStart
    //event without needing a player id
    for(let player of this.players){
      this.cardEvaluator.evaluatePlayerEvent('turnEnd', player.id);
    }

    // do it
    var currentTurn = this.turnState.passTurn();

    //fire off the event service now so the times won't be impacted by processing the events
    this.gameEventService.startTurnEnergyTimer(currentTurn);

    //give some handouts
    for(let player of this.players){
      let max = this.playerResourceState.incriment(player.id, 1);
      this.playerResourceState.reset(player.id);
      let current = this.playerResourceState.adjust(player.id, 1);
      action.playerResources.push({
        playerId: player.id,
        current,
        max
      });

      //update stats
      this.statsState.setStat('COMBOCOUNT', 0, player.id);

      //and finally eval the new turn
      this.cardEvaluator.evaluatePlayerEvent('turnStart', player.id);

      queue.push(new DrawCard(player.id));
    }
    action.currentTurn = currentTurn;

    queue.complete(action);
  }
}
