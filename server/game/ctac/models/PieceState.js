import _ from 'lodash';
import GamePiece from './GamePiece.js';

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

  newFromCard(cardDirectory, cardTemplateId, playerId, position){
    let cardPlayed = cardDirectory.directory[cardTemplateId];

    var newPiece = new GamePiece();

    //assign the next id to the piece before it's spawned so any actions can reference the piece
    newPiece.id = this.nextId();
    newPiece.position = position;
    newPiece.playerId = playerId;
    newPiece.name = cardPlayed.name;
    newPiece.cardTemplateId = cardTemplateId;
    newPiece.attack = cardPlayed.attack;
    newPiece.health = cardPlayed.health;
    newPiece.baseAttack = cardPlayed.attack;
    newPiece.baseHealth = cardPlayed.health;
    newPiece.movement = cardPlayed.movement;
    newPiece.baseMovement = cardPlayed.movement;
    newPiece.baseTags = cardPlayed.tags;
    newPiece.tags = cardPlayed.tags;
    newPiece.statuses = cardPlayed.statuses || 0;

    return newPiece;
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
