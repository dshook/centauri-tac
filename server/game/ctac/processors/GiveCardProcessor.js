import GiveCard from '../actions/GiveCard.js';
import loglevel from 'loglevel-decorator';
import _ from 'lodash';

@loglevel
export default class GiveCardProcessor
{
  constructor(cardState, cardEvaluator, cardDirectory)
  {
    this.cardState = cardState;
    this.cardDirectory = cardDirectory;
    this.cardEvaluator = cardEvaluator;
  }

  /**
   * Proc
   */
  async handleAction(action, queue)
  {
    if (!(action instanceof GiveCard)) {
      return;
    }

    let givenCard = this.cardDirectory.newFromId(action.cardTemplateId);
    this.cardState.addToHand(action.playerId, givenCard);

    action.cardId = givenCard.id;

    queue.complete(action);
    this.log.info('player %s was given card %s',
      action.playerId, action.cardTemplateId);
  }
}
