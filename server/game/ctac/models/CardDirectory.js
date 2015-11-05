import Card from './Card.js';
import _ from 'lodash';

/**
 * A deep repository of knowledge 
 */
export default class CardDirectory
{
  constructor()
  {
    this.directory = {};
  }

  get cardIds()
  {
    return Object.keys(this.directory);
  }

  add(card){
    var c = new Card();
    //copy props over to proper object
    for(var k in card) c[k]=card[k];

    this.directory[c.id] = c;
  }

  getByTag(tag){
    return _.filter(this.directory, c => c.tags && c.tags.indexOf(tag) != -1);
  }
}
