/**
 * Current state of all the pieces
 */
export default class PieceState
{
  constructor()
  {
    this.pieces = [];
  }

  piece(id){
    return this.pieces.filter(x => x.id == id)[0];
  }
}
