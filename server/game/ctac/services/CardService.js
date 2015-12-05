import loglevel from 'loglevel-decorator';
import CardDirectory from '../models/CardDirectory.js';
import CardLang from '../../../../lang/cardlang.js';
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
    let parser = CardLang.parser;

    for(let cardFileName in cardRequires){
      let card = cardRequires[cardFileName];
      if(card.eventcode){
        try{
          let cardEvents = parser.parse(card.eventcode);
          card.events = cardEvents;
        }catch(e){
          this.log.info('Error parsing card text %s %s', card.events, e);
          //throw again so you don't run the server with a bad card
          throw e;
        }
      }else{
        card.events = null;
      }
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

    app.registerInstance('selector', app.make(Selector));
    app.registerInstance('cardEvaluator', app.make(CardEvaluator));
  }
}
