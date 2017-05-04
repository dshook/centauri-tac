import Card from './Card.js';
import CardLang from '../../../../lang/cardlang.js';
import loglevel from 'loglevel-decorator';
import deepFreeze from 'deep-freeze';
import fs from 'fs';
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

    var cardRequires = {};

    //NAS-T  required for heroku it seems
    var directoryPath = __dirname + '/../../../../cards';
    fs.readdirSync(directoryPath).map(function (filename) {
      let contents = fs.readFileSync(directoryPath + "/" + filename, "utf8");
      try{
        cardRequires[filename] = JSON.parse(contents.replace(/[\t\r\n]/g, ''));
      }catch(e){
        this.log.error('Error loading card ' + filename, e, e.stack);
      }
    })

    for(let cardFileName in cardRequires){
      try{
        let card = cardRequires[cardFileName];
        this.add(card);
      }catch(e){
        this.log.error('Error registering card ' + cardFileName, e, e.stack);
        throw e;
      }
    }
    this.log.info('Registered %s cards', Object.keys(this.directory).length);
  }

  get cardIds()
  {
    return Object.keys(this.directory);
  }

  add(card){
    var c = new Card();
    //copy props over to proper object
    for(var k in card) c[k] = card[k];

    //base prop(s)
    c.baseCost = c.cost;

    if(card.eventcode){
      try{
        let cardEvents = this.parser.parse(card.eventcode);
        c.events = cardEvents;
      }catch(e){
        let message = `Unable to parse card ${card.name} : ${e.message}`;
        this.log.error(message);
        //throw again so you don't run the server with a bad card
        throw message;
      }
    }else{
      c.events = null;
    }

    if(this.directory[c.cardTemplateId]){
      throw `Duplicate card template id ${c.cardTemplateId} used by ${c.name}`;
    }

    this.directory[c.cardTemplateId] = deepFreeze(c);
  }

  getByTag(tags){
    //ensure array if single string is passed
    tags = [].concat(tags);
    return _.filter(this.directory, c => c.tags && _.intersection(c.tags, tags).length > 0 );
  }

  newFromId(cardTemplateId){
    let directoryCard = this.directory[cardTemplateId];
    //clone into new card
    var cardClone = new Card();
    for(var k in directoryCard) cardClone[k]=directoryCard[k];

    //gotta clone so we get new object ref that isn't frozen
    cardClone.buffs = _.cloneDeep(directoryCard.buffs);

    return cardClone;
  }
}
