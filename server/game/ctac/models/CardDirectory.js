import Card from './Card.js';
import CardLang from '../../../../lang/cardlang.js';
import Statuses from '../models/Statuses.js';
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
    for(var k in card) c[k] = card[k];

    if(card.eventcode){
      try{
        let cardEvents = this.parser.parse(card.eventcode);
        c.events = cardEvents;

        //copy over each event as a tag
        c.tags = c.tags.concat(_.map(c.events, 'event'));
      }catch(e){
        this.log.error('Error parsing card text %s %s', card.events, e);
        //throw again so you don't run the server with a bad card
        throw `Unable to parse card ${card.name} with text ${card.eventcode} ${e.message}`;
      }
    }else{
      c.events = null;
    }

    this.directory[c.cardTemplateId] = c;
  }

  getByTag(tags){
    //ensure array if single string is passed
    tags = [].concat(tags);
    return _.filter(this.directory, c => c.tags && _.intersection(c.tags, tags).length > 0 );
  }
}
