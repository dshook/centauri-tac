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

  remove(id){
    let index = this.pieces.indexOf(this.piece(id));
    this.pieces.splice(index, 1);
  }
}
