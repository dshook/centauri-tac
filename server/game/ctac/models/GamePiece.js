import Position from './Position.js';

export default class GamePiece
{
  constructor()
  {
    this.id = null;
    this.cardId = null;
    this.playerId = null;
    this.position = new Position();
    this.attack = null;
    this.baseAttack = null;
    this.health = null;
    this.baseHealth = null;
    this.movement = null;
    this.baseMovement = null;
    this.tags = [];
    this.buffs = [];
  }
}
