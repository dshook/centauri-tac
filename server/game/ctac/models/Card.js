export default class Card
{
  constructor(){
    this.id = null; //id of instance of card
    this.cardTemplateId = null; //id of template card
    this.playerId = null;
    this.name = null;
    this.description = null;
    this.cost = null;
    this.baseCost = null;
    this.attack = null;
    this.health = null;
    this.movement = null;
    this.range = null;
    this.spellDamage = null;
    this.tags = [];
    this.buffs = [];
    this.statuses = 0;
    this.rarity = 0;
    this.race = 0;

    this.uncollectible = false;
    this.inHand = false,
    this.inDeck = false;
  }

  hasTag(tag){
    return this.tags.indexOf(tag) != -1;
  }

  get isSpell(){
    return this.tags.includes('Spell');
  }

  get isMinion(){
    return this.tags.includes('Minion');
  }

  //only cost supported right now
  addBuff(buff){
    let action = {};
    for(let attrib of ['cost']){
      if(buff[attrib] == null) continue;

      let capAttr = attrib.charAt(0).toUpperCase() + attrib.slice(1);

      let origStat = this[attrib];
      this[attrib] += buff[attrib];

      //cap cost at 0
      if(attrib === 'cost'){
        this[attrib] = Math.max(0, this[attrib]);
      }

      //update action with new values
      let newAttrib = 'new' + capAttr;
      action[newAttrib] = this[attrib];
      action[attrib] = action[newAttrib] - origStat;
    }
    this.buffs.push(buff);

    return action;
  }

  //requires you to find the buff instance beforehand
  removeBuff(buff){
    if(this.buffs.length === 0) return null;

    let buffIndex = this.buffs.indexOf(buff);
    if(buffIndex === -1) return null;
    this.buffs.splice(buffIndex, 1);

    let action = {};
    for(let attrib of ['cost']){
      if(buff[attrib] == null) continue;

      let capAttr = attrib.charAt(0).toUpperCase() + attrib.slice(1);
      let baseAttr = 'base' + capAttr;

      let origStat = this[attrib];
      this[attrib] -= buff[attrib];

      //cap at min of 0 to prevent negative attack/movement
      this[attrib] = Math.max(0, this[attrib]);

      //but for health, only lower to min of 1 so you can't kill off a piece by debuff
      if(attrib === 'health'){
        this[attrib] = Math.max(1, this[attrib]);
      }

      //update action with new values
      let newAttrib = 'new' + capAttr;
      action[newAttrib] = this[attrib];
      action[attrib] = action[newAttrib] - origStat;
      action[baseAttr] = this[baseAttr];
    }

    return action;
  }
}