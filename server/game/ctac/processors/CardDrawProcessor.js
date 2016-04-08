import GamePiece from '../models/GamePiece.js';
import DrawCard from '../actions/DrawCard.js';
import PieceHealthChange from '../actions/PieceHealthChange.js';
import loglevel from 'loglevel-decorator';

/**
 * Handle the PassTurn action
 */
@loglevel
export default class CardDrawProcessor
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
    if (!(action instanceof DrawCard)) {
      return;
    }

    let playerDeck = this.cardState.decks[action.playerId];

    if(playerDeck.length == 0){
      this.log.warn('No cards to draw for player %s', action.playerId);

      action.milled = true;
      let hero = this.pieceState.hero(action.playerId);
      this.cardState.millState[action.playerId] -= 1;
      queue.push(new PieceHealthChange(hero.id, this.cardState.millState[action.playerId]));
      return queue.cancel(action, true);
    }

    let cardDrawn = this.cardState.drawCard(action.playerId);

    action.cardId = cardDrawn.id;
    action.cardTemplateId = cardDrawn.cardTemplateId;

    this.cardEvaluator.evaluatePlayerEvent('cardDrawn', action.playerId);

    queue.complete(action);
    this.log.info('player %s drew card %s %s',
      action.playerId, cardDrawn.cardTemplateId, cardDrawn.name);
  }
}
