import ShuffleToDeck from '../actions/ShuffleToDeck.js';
import loglevel from 'loglevel-decorator';
import _ from 'lodash';

@loglevel
export default class ShuffleToDeckProcessor
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
    if (!(action instanceof ShuffleToDeck)) {
      return;
    }

    let givenCard = this.cardDirectory.newFromId(action.cardTemplateId);
    this.cardState.addToDeck(action.playerId, givenCard, true);

    action.cardId = givenCard.id;

    queue.complete(action);
    this.log.info('player %s shuffled card %s into deck',
      action.playerId, action.cardTemplateId);
  }
}
