import Card from './Card.js';
import CardLang from '../../../../lang/cardlang.js';
import loglevel from 'loglevel-decorator';
import _ from 'lodash';

/**
 * A deep repository of knowledge 
 */
 @loglevel
export default class CardDirectory
{
  constructor()
  {
    this.directory = {};
    this.parser = CardLang.parser;
  }

  get cardIds()
  {
    return Object.keys(this.directory);
  }

  add(card){
    var c = new Card();
    //copy props over to proper object
    for(var k in card) c[k]=card[k];

    if(card.eventcode){
      try{
        let cardEvents = this.parser.parse(card.eventcode);
        c.events = cardEvents;
      }catch(e){
        this.log.info('Error parsing card text %s %s', card.events, e);
        //throw again so you don't run the server with a bad card
        throw `Unable to parse card ${card.name} with text ${card.eventcode} ${e.message}`;
      }
    }else{
      c.events = null;
    }

    this.directory[c.id] = c;
  }

  getByTag(tag){
    return _.filter(this.directory, c => c.tags && c.tags.indexOf(tag) != -1);
  }
}
