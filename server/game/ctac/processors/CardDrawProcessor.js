import GamePiece from '../models/GamePiece.js';
import DrawCard from '../actions/DrawCard.js';
import loglevel from 'loglevel-decorator';

/**
 * Handle the PassTurn action
 */
@loglevel
export default class CardDrawProcessor
{
  constructor(cardDirectory, decks, hands)
  {
    this.cardDirectory = cardDirectory;
    this.decks = decks;
    this.hands = hands;
  }

  /**
   * Proc
   */
  async handleAction(action, queue)
  {
    if (!(action instanceof DrawCard)) {
      return;
    }

    let playerDeck = this.decks[action.playerId];
    let playerHand = this.hands[action.playerId];

    let cardDrawn = playerDeck.splice(0, 1)[0];
    playerHand.push(cardDrawn);

    action.cardId = cardDrawn.id;

    queue.complete(action);
    this.log.info('player %s drew card %s %s',
      action.playerId, cardDrawn.id, cardDrawn.name);
  }
}
