import Position from './Position.js';
import Direction from './Direction.js';

export default class GamePiece
{
  constructor()
  {
    this.id = null;
    this.cardTemplateId = null;
    this.name = null;
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
    this.statuses = 0;
    this.abilityCharge = 0;

    //piece has moved or attacked this turn?
    this.hasMoved = false;
    this.hasAttacked = false;

    this.bornOn = null;
  }

  //requires you to find the buff instance beforehand
  removeBuff(buff){
    if(this.buffs.length === 0) return null;

    let buffIndex = this.buffs.indexOf(buff);
    if(buffIndex === -1) return null;
    this.buffs.splice(buffIndex, 1);

    let attribs = ['attack', 'health', 'movement', 'range'];
    let action = {};
    for(let attrib of attribs){
      if(buff[attrib] == null) continue;

      let origStat = this[attrib];
      this[attrib] -= buff[attrib];

      //cap at min of 0 to prevent negative attack/movement
      this[attrib] = Math.max(0, this[attrib]);

      //update action with new values
      let newAttrib = 'new' + attrib.charAt(0).toUpperCase() + attrib.slice(1);
      action[newAttrib] = this[attrib];
      action[attrib] = action[newAttrib] - origStat;
    }

    return action;
  }
}
