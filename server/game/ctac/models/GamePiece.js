import Position from './Position.js';

export default class GamePiece
{
  constructor()
  {
    this.id = null;
    this.resourceId = null;
    this.playerId = null;
    this.position = new Position();
  }

}