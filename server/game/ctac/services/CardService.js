import loglevel from 'loglevel-decorator';
import CardDirectory from '../models/CardDirectory.js';
import CardState from '../models/CardState.js';
import Selector from '../cardlang/Selector.js';
import CardEvaluator from '../cardlang/CardEvaluator.js';

/**
 * Expose the cards and activate card processor
 */
@loglevel
export default class CardService
{
  constructor(container, queue)
  {
    var cardDirectory = new CardDirectory();
    container.registerValue('cardDirectory', cardDirectory);

    //cards in hand indexed by player id
    var cardState = new CardState();
    container.registerValue('cardState', cardState);

    container.registerSingleton('selector', Selector);
    container.registerSingleton('cardEvaluator', CardEvaluator);
  }
}
