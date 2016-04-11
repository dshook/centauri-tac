//Whenever a piece gets flat up destroyed, and didn't die from taking damage
export default class PieceDestroyed
{
  constructor(pieceId)
  {
    this.id = null;
    this.pieceId = pieceId;
  }
}
