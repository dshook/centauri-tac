import loglevel from 'loglevel-decorator';
import ActivateCardProcessor from '../processors/ActivateCardProcessor.js';
import requireDir from 'require-dir';


/**
 * Expose the turn model and add the processor to the action pipeline
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
