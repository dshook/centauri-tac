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

  add(newPiece){
    let nextId = this.pieces.length == 0 ? 1 :
       _.max(this.pieces, x => x.id).id + 1;
    newPiece.id = nextId;

    this.pieces.push(newPiece);

    return newPiece.id;
  }

  remove(id){
    let index = this.pieces.indexOf(this.piece(id));
    this.pieces.splice(index, 1);
  }

  piece(id){
    return this.pieces.filter(x => x.id == id)[0];
  }

  hero(playerId){
    return this.pieces.filter(x => x.playerId == playerId && x.tags[0] === 'Hero')[0];
  }
}
