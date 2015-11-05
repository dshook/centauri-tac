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
    this.directory[card.id] = card;
  }

  getByTag(tag){
    return _.filter(this.directory, c => c.tags && c.tags.indexOf(tag) != -1);
  }
}
