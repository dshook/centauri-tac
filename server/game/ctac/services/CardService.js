import loglevel from 'loglevel-decorator';
import CardDirectory from '../models/CardDirectory.js';
import CardState from '../models/CardState.js';
import requireDir from 'require-dir';
import Selector from '../cardlang/Selector.js';
import CardEvaluator from '../cardlang/CardEvaluator.js';

/**
 * Expose the cards and activate card processor
 */
@loglevel
export default class CardService
{
  constructor(app, queue)
  {
    var cardRequires = requireDir('../../../../cards');
    var cardDirectory = new CardDirectory();

    for(let cardFileName in cardRequires){
      let card = cardRequires[cardFileName];
      cardDirectory.add(card);
    }
    this.log.info('Registered cards %j', cardDirectory.directory);
    app.registerInstance('cardDirectory', cardDirectory);

    //cards in hand indexed by player id
    var cardState = new CardState();
    app.registerInstance('cardState', cardState);

    app.registerInstance('selector', app.make(Selector));
    app.registerInstance('cardEvaluator', app.make(CardEvaluator));
  }
}
