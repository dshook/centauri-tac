import _ from 'lodash';
import Position from './Position.js';
import Direction from './Direction.js';
import attributes from '../util/Attributes.js';

export default class GamePiece
{
  constructor()
  {
    this.id = null;
    this.cardTemplateId = null;
    this.name = null;
    this.description = null;
    this.playerId = null;
    this.position = new Position();
    this.direction = Direction.South;
    this.attack = null;
    this.baseAttack = null;
    this.health = null;
    this.baseHealth = null;
    this.movement = null;
    this.baseMovement = null;
    this.range = null;
    this.baseRange = null;
    this.events = null;
    this.baseTags = [];
    this.tags = [];
    this.buffs = [];
    this.aura = null;
    this.statuses = 0;
    this.baseStatuses = 0;
    this.abilityCharge = 0;
    this.armor = 0;

    //piece has moved or attacked this turn?
    this.hasMoved = false;
    this.attackCount = 0;

    this.bornOn = null;
  }

  get maxBuffedHealth(){
    return this.baseHealth + _.sum(this.buffs, 'health');
  }

  addBuff(buff){
    let action = {};
    for(let attrib of attributes){
      if(buff[attrib] == null) continue;

      let capAttr = attrib.charAt(0).toUpperCase() + attrib.slice(1);

      let origStat = this[attrib];
      this[attrib] += buff[attrib];

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
    for(let attrib of attributes){
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
