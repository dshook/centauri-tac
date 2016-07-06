import GamePiece from '../models/GamePiece.js';
import DiscardCard from '../actions/DiscardCard.js';
import PieceHealthChange from '../actions/PieceHealthChange.js';
import loglevel from 'loglevel-decorator';
import _ from 'lodash';

@loglevel
export default class DiscardCardProcessor
{
  constructor(cardState, cardEvaluator, pieceState)
  {
    this.cardState = cardState;
    this.cardEvaluator = cardEvaluator;
    this.pieceState = pieceState;
  }

  /**
   * Proc
   */
  async handleAction(action, queue)
  {
    if (!(action instanceof DiscardCard)) {
      return;
    }

    let playerHand = this.cardState.hands[action.playerId];

    if(playerHand.length == 0){
      this.log.info('No cards to discard for player %s', action.playerId);
      return queue.cancel(action);
    }

    let cardDiscarded = _.sample(playerHand);

    action.cardId = cardDiscarded.id;

    //"play" the card in card state to remove it
    this.cardState.playCard(action.playerId, cardDiscarded.id);

    this.cardEvaluator.evaluatePlayerEvent('cardDiscarded', action.playerId);

    queue.complete(action);
    this.log.info('player %s discarded card %s %s',
      action.playerId, cardDiscarded.id, cardDiscarded.name);
  }
}
