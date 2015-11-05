import loglevel from 'loglevel-decorator';
import CardDirectory from '../models/CardDirectory.js';
import ActivateCardProcessor from '../processors/ActivateCardProcessor.js';
import CardDrawProcessor from '../processors/CardDrawProcessor.js';
import SpawnDeckProcessor from '../processors/SpawnDeckProcessor.js';
import requireDir from 'require-dir';

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
    var hands = {};
    app.registerInstance('hands', hands);
    //cards in deck indexed by player id
    var decks = {};
    app.registerInstance('decks', decks);

    queue.addProcessor(SpawnDeckProcessor);
    queue.addProcessor(ActivateCardProcessor);
    queue.addProcessor(CardDrawProcessor);
  }
}
