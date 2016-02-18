import _ from 'lodash';

/**
 * Current state of all the pieces
 */
export default class PieceState
{
  constructor()
  {
    this.pieces = [];
  }

  nextId(){
    return this.pieces.length == 0 ? 1 :
      _.max(this.pieces, x => x.id).id + 1;
  }

  add(newPiece){
    newPiece.id = this.nextId();

    this.pieces.push(newPiece);

    return newPiece.id;
  }

  remove(id){
    let index = this.pieces.indexOf(this.piece(id));
    this.pieces.splice(index, 1);
  }

  piece(id){
    return this.pieces.find(x => x.id == id);
  }

  pieceAt(x, z){
    return this.pieces.find(p => p.position.x === x && p.position.z === z);
  }

  hero(playerId){
    return this.pieces.find(x => x.playerId == playerId && x.tags[0] === 'Hero');
  }
}
