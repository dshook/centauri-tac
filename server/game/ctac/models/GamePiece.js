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
    this.health = null;
    this.movement = null;
  }

}