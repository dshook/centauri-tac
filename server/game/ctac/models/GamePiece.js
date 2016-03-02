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
    this.baseTags = [];
    this.tags = [];
    this.buffs = [];
  }
}
