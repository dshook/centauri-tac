import _ from 'lodash';
import Position from './Position.js';
import Direction from './Direction.js';
import Statuses from './Statuses.js';
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
    this.spellDamage = null;
    this.baseSpellDamage = null;
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
    this.moveCount = 0;
    this.attackCount = 0;

    this.bornOn = null;
  }

  get maxBuffedHealth(){
    return this.baseHealth + _.sumBy(this.buffs, 'health');
  }
  get isHero(){
    return this.tags.includes('Hero');
  }

  get isMinion(){
    return this.tags.includes('Minion');
  }

  addBuff(buff, cardEvaluator){
    let action = this.addBuffStats(buff, cardEvaluator);
    this.buffs.push(buff);

    return action;
  }

  //requires you to find the buff instance beforehand
  removeBuff(buff, cardEvaluator){
    if(this.buffs.length === 0) return null;

    let buffIndex = this.buffs.indexOf(buff);
    if(buffIndex === -1) return null;
    this.buffs.splice(buffIndex, 1);

    return this.removeBuffStats(buff, cardEvaluator);
  }

  enableBuff(buff, cardEvaluator){
    if(buff.enabled) return;

    buff.enabled = true;
    return this.addBuffStats(buff, cardEvaluator);
  }

  disableBuff(buff, cardEvaluator){
    if(!buff.enabled) return;

    buff.enabled = false;
    return this.removeBuffStats(buff, cardEvaluator);
  }

  addBuffStats(buff, cardEvaluator){
    let action = {};
    for(let attrib of attributes){
      if(buff[attrib] == null) continue;

      let capAttr = attrib.charAt(0).toUpperCase() + attrib.slice(1);

      let origStat = this[attrib];
      this[attrib] += buff[attrib];

      //cap at min of 0 to prevent negative attack/movement
      this[attrib] = Math.max(0, this[attrib]);

      //update action with new values
      let newAttrib = 'new' + capAttr;
      action[newAttrib] = this[attrib];
      action[attrib] = action[newAttrib] - origStat;
    }
    if(buff.addStatus){
      let statusAction = this.addStatuses(buff.addStatus, cardEvaluator);
      //merge the status action into the buff action, the attribute changes from status should be the most up to date
      Object.assign(action, statusAction);
    }
    if(buff.removeStatus){
      let statusAction = this.removeStatuses(buff.removeStatus, cardEvaluator);
      Object.assign(action, statusAction);
    }
    return action
  }

  removeBuffStats(buff, cardEvaluator){
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

    //now for statuses, if one was added, remove it, and vice versa
    if(buff.addStatus){
      let statusAction = this.removeStatuses(buff.addStatus, cardEvaluator);
      Object.assign(action, statusAction);
    }
    if(buff.removeStatus){
      let statusAction = this.addStatuses(buff.removeStatus, cardEvaluator);
      //merge the status action into the buff action, the attribute changes from status should be the most up to date
      Object.assign(action, statusAction);
    }

    return action;
  }

  addStatuses(add, cardEvaluator){
    let action = {};

    if(this.range){
      if((add & Statuses.Piercing) || (add & Statuses.Cleave)){
        this.log.info('Ignoring pierce or cleave add to ranged piece %s', action.pieceId);
        action.addStatus = action.addStatus & ~Statuses.Piercing;
        action.addStatus = action.addStatus & ~Statuses.Cleave;
      }
    }

    this.statuses = this.statuses | add;

    //remove all statuses other than silence if it was silenced
    if(add & Statuses.Silence){
      action.removeStatus = this.statuses & ~Statuses.Silence;
      this.statuses = Statuses.Silence;

      //back out any buffs on the this
      if(this.buffs.length > 0){

        for(let b = this.buffs.length - 1; b >= 0; b--){
          let buff = this.buffs[b];
          let buffChange = this.removeBuff(buff);

          if(!buffChange){
            this.log.error('Cannot unbuff piece %j with buff %j', this, buff);
            continue;
          }

          for(let attrib of attributes){
            let newAttrib = 'new' + attrib.charAt(0).toUpperCase() + attrib.slice(1);
            action[attrib] = buffChange[attrib];
            action[newAttrib] = buffChange[newAttrib];

            this.log.info('un buffing piece %s to %s %s', this.id, this[attrib], attrib);
          }
        }
      }

      //aura and events go bye bye
      this.aura = null;
      this.events = null;

      //remove timers for this piece
      cardEvaluator.cleanupTimers(this);
    }
    action.statuses = this.statuses;

    return action;
  }

  removeStatuses(remove, cardEvaluator){
    this.statuses = this.statuses & ~remove;
    return {
      removeStatus: remove,
      statuses: this.statuses
    };
  }
}
