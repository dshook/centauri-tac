export default class Card
{
  constructor(){
    this.id = null; //id of instance of card
    this.cardTemplateId = null; //id of template card
    this.name = null;
    this.description = null;
    this.cost = null;
    this.attack = null;
    this.health = null;
    this.movement = null;
    this.range = null;
    this.tags = [];
    this.statuses = 0;
  }

  hasTag(tag){
    return this.tags.indexOf(tag) != -1;
  }
}