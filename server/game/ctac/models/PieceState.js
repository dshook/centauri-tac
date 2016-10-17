import _ from 'lodash';
import GamePiece from './GamePiece.js';
import Statuses from './Statuses.js';

/**
 * Current state of all the pieces
 */
export default class PieceState
{
  constructor()
  {
    this.reset();
  }

  reset(){
    this.pieces = [];
    this.nextPieceId = 1;
  }

  nextId(){
    return this.nextPieceId++;
  }

  add(newPiece, turn){
    newPiece.id = this.nextId();
    newPiece.bornOn = turn;

    this.pieces.push(newPiece);

    return newPiece.id;
  }

  newFromCard(cardDirectory, cardTemplateId, playerId, position){
    let cardPlayed = cardDirectory.directory[cardTemplateId];

    var newPiece = new GamePiece();

    //assign the next id to the piece before it's spawned so any actions can reference the piece
    newPiece.id = this.nextPieceId;
    newPiece.position = position;
    newPiece.playerId = playerId;

    this.copyPropertiesFromCard(newPiece, cardPlayed);

    this.setInitialMoveAttackStatus(newPiece);

    //server only tracking vars
    newPiece.bornOn = null;

    return newPiece;
  }

  copyPropertiesFromCard(piece, card){
    piece.name = card.name;
    piece.description = card.description;
    piece.cardTemplateId = card.cardTemplateId;
    piece.events = _.cloneDeep(card.events);
    piece.attack = card.attack;
    piece.health = card.health;
    piece.baseAttack = card.attack;
    piece.baseHealth = card.health;
    piece.movement = card.movement;
    piece.baseMovement = card.movement;
    piece.range = card.range || null;
    piece.baseRange = card.range || null;
    piece.spellDamage = card.spellDamage || null;
    piece.baseSpellDamage = card.spellDamage || null;
    piece.baseTags = _.cloneDeep(card.tags);
    piece.tags = _.cloneDeep(card.tags);
    piece.statuses = card.statuses || 0;
    piece.baseStatuses = card.statuses || 0;
  }

  copyPropertiesFromPiece(src, dest){
    dest.name = src.name;
    dest.cardTemplateId = src.cardTemplateId;
    dest.events = _.cloneDeep(src.events);
    dest.attack = src.attack;
    dest.baseAttack = src.baseAttack;
    dest.health = src.health;
    dest.baseHealth = src.baseHealth;
    dest.movement = src.movement;
    dest.baseMovement = src.movement;
    dest.range = src.range;
    dest.baseRange = src.baseRange;
    dest.spellDamage = src.spellDamage;
    dest.baseSpellDamage = src.baseSpellDamage;
    dest.tags = _.cloneDeep(src.tags);
    dest.baseTags = _.cloneDeep(src.tags);
    dest.statuses = src.statuses;
    dest.baseStatuses = src.baseStatuses;
    dest.abilityCharge = src.abilityCharge;
    dest.buffs = _.cloneDeep(src.buffs);

    //copy aura but maintain dest piece Id
    dest.aura = _.cloneDeep(src.aura);
    if(dest.aura){
      dest.aura.pieceId = dest.id;
    }
  }

  setInitialMoveAttackStatus(piece){
    piece.hasMoved = !(piece.statuses & Statuses.Charge) && piece.range === null;
    piece.attackCount = (piece.statuses & Statuses.Charge) ? 0 : 9;
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

  withStatus(status){
    return this.pieces.filter(p => p.statuses & status);
  }

  hero(playerId){
    return this.pieces.find(x => x.playerId == playerId && x.tags[0] === 'Hero');
  }

  totalSpellDamage(playerId){
    return this.pieces
      .filter(x => x.playerId == playerId && x.spellDamage)
      .map(p => p.spellDamage)
      .reduce((a, b) => { return a + b; }, 0);
  }
}
