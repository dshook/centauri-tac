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
  constructor(container)
  {
    container.registerSingleton('cardDirectory', CardDirectory);
    container.registerSingleton('cardState', CardState);

    container.registerSingleton('selector', Selector);
    container.registerSingleton('cardEvaluator', CardEvaluator);
  }
}
