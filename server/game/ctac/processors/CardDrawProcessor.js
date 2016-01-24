import GamePiece from '../models/GamePiece.js';
import DrawCard from '../actions/DrawCard.js';
import loglevel from 'loglevel-decorator';

/**
 * Handle the PassTurn action
 */
@loglevel
export default class CardDrawProcessor
{
  constructor(cardState, cardEvaluator)
  {
    this.cardState = cardState;
    this.cardEvaluator = cardEvaluator;
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
      this.log.info('No cards to draw for player %s', action.playerId);
      //TODO: better handling of out of cards
      queue.cancel(action);
      return;
    }

    let cardDrawn = this.cardState.drawCard(action.playerId);

    //kinda confusing since we have to refer to both the id of the card and the template id
    action.cardId = cardDrawn.id;
    action.cardTemplateId = cardDrawn.cardId;

    this.cardEvaluator.evaluatePlayerEvent('cardDrawn', action.playerId);

    queue.complete(action);
    this.log.info('player %s drew card %s %s',
      action.playerId, cardDrawn.cardId, cardDrawn.name);
  }
}
