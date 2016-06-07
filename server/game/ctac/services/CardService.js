import loglevel from 'loglevel-decorator';
import CardDirectory from '../models/CardDirectory.js';
import CardState from '../models/CardState.js';
import Selector from '../cardlang/Selector.js';
import CardEvaluator from '../cardlang/CardEvaluator.js';
import fs from 'fs';

/**
 * Expose the cards and activate card processor
 */
@loglevel
export default class CardService
{
  constructor(app, queue)
  {
    var cardDirectory = new CardDirectory('../../../../cards');

    app.registerInstance('cardDirectory', cardDirectory);

    //cards in hand indexed by player id
    var cardState = new CardState();
    app.registerInstance('cardState', cardState);

    app.registerInstance('selector', app.make(Selector));
    app.registerInstance('cardEvaluator', app.make(CardEvaluator));
  }
}
