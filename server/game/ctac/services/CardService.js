import loglevel from 'loglevel-decorator';
import ActivateCardProcessor from '../processors/ActivateCardProcessor.js';
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
    var cardDirectory = {};

    for(let cardFileName in cardRequires){
      let card = cardRequires[cardFileName];
      cardDirectory[card.id] = card;
    }
    this.log.info('Registered cards %j', cardDirectory);
    app.registerInstance('cardDirectory', cardDirectory);

    queue.addProcessor(ActivateCardProcessor);
  }
}
