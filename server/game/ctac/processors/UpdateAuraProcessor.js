import GamePiece from '../models/GamePiece.js';
import PieceBuff from '../actions/PieceBuff.js';
import CardBuff from '../actions/CardBuff.js';
import PieceAura from '../actions/PieceAura.js';
import CardAura from '../actions/CardAura.js';
import UpdateAura from '../actions/UpdateAura.js';
import loglevel from 'loglevel-decorator';
import attributes from '../util/Attributes.js';
import {Intersection, Difference} from '../util/SetOps.js';

/**
 * Update all auras based off the current piece state,
   this gets fired off at the end of the initial queue processing
 */
@loglevel
export default class UpdateAuraProcessor
{
  constructor(pieceState, cardEvaluator, selector, cardState)
  {
    this.pieceState = pieceState;
    this.cardState = cardState;
    this.cardEvaluator = cardEvaluator;
    this.selector = selector;

    //keep track of which pieces we've already 'activated'
    this.auraPieces = [];
    //ditto for card aura's
    this.cardAuraPieces = [];
  }

  /**
   * Proc
   */
  async handleAction(action, queue)
  {
    if (action != null && !(action instanceof UpdateAura)) {
      return;
    }

    this.updatePieces(queue);
    this.updateCards(queue);

    this.log.info('Updated auras');
  }

  updatePieces(queue){

    let auraPieces = this.pieceState.pieces.filter(p => p.aura != null && (p.aura instanceof PieceAura));

    //find out newly added pieces
    let newlyAdded = Difference(auraPieces, this.auraPieces, (a,b) => a.id === b.id);

    //add buffs to all selected pieces
    for(let newAuraPiece of newlyAdded){
      let aura = newAuraPiece.aura;
      let pieceSelectorParams = {
        selfPiece: newAuraPiece,
        controllingPlayerId: newAuraPiece.playerId,
        activatingPiece: newAuraPiece,
        position: newAuraPiece.position,
        isSpell: false
      };
      var selected = this.selector.selectPieces(aura.pieceSelector, pieceSelectorParams);
      //set up a new buff for each selected piece that has all the attributes of the buff
      this.addBuffs(queue, newAuraPiece, selected, pieceSelectorParams, PieceBuff);
    }

    //find removed pieces
    let removedPieces = Difference(this.auraPieces, auraPieces, (a,b) => a.id === b.id);
    for(let oldAuraPiece of removedPieces){
      let aura = oldAuraPiece.aura;
      let affectedPieces = this.pieceState.pieces.filter(p => p.buffs.some(b => b.auraPieceId == oldAuraPiece.id));
      //remove buffs from all piece that had a buff from that piece
      this.removeBuffs(queue, oldAuraPiece, affectedPieces, PieceBuff);
    }

    //find remaining pieces that still might have moved/changed
    let remainingPieces = Intersection(this.auraPieces, auraPieces, (a,b) => a.id === b.id);
    for(let remainingPiece of remainingPieces){
      //find all affected pieces, compare with affected pieces last process
      let aura = remainingPiece.aura;
      let pieceSelectorParams = {
        selfPiece: remainingPiece,
        controllingPlayerId: remainingPiece.playerId,
        activatingPiece: remainingPiece,
        position: remainingPiece.position,
        isSpell: false
      };
      var selected = this.selector.selectPieces(aura.pieceSelector, pieceSelectorParams);

      var previouslySelected = this.pieceState.pieces.filter(p => p.buffs.some(b => b.auraPieceId == remainingPiece.id));

      //add or remove buffs as necessary
      let newComers = Difference(selected, previouslySelected, (a,b) => a.id === b.id);
      this.addBuffs(queue, remainingPiece, newComers, pieceSelectorParams, PieceBuff);

      let leavers = Difference(previouslySelected, selected, (a,b) => a.id === b.id);
      this.removeBuffs(queue, remainingPiece, leavers, PieceBuff);
    }

    this.auraPieces = auraPieces;
  }

  updateCards(queue){

    let cardAuraPieces = this.pieceState.pieces.filter(p => p.aura != null && (p.aura instanceof CardAura));

    //find out newly added pieces
    let newlyAdded = Difference(cardAuraPieces, this.cardAuraPieces, (a,b) => a.id === b.id);

    //add buffs to all selected pieces
    for(let newAuraPiece of newlyAdded){
      let aura = newAuraPiece.aura;
      let pieceSelectorParams = {
        selfPiece: newAuraPiece,
        controllingPlayerId: newAuraPiece.playerId,
        activatingPiece: newAuraPiece,
        position: newAuraPiece.position,
        isSpell: false
      };
      var selected = this.selector.selectCards(aura.cardSelector, pieceSelectorParams);
      //set up a new buff for each selected piece that has all the attributes of the buff
      this.addBuffs(queue, newAuraPiece, selected, pieceSelectorParams, CardBuff);
    }

    //find removed pieces
    let removedPieces = Difference(this.cardAuraPieces, cardAuraPieces, (a,b) => a.id === b.id);
    for(let oldAuraPiece of removedPieces){
      let aura = oldAuraPiece.aura;
      let affectedPieces = this.pieceState.pieces.filter(p => p.buffs.some(b => b.auraPieceId == oldAuraPiece.id));
      //remove buffs from all piece that had a buff from that piece
      this.removeBuffs(queue, oldAuraPiece, affectedPieces, CardBuff);
    }

    //find remaining pieces that still might have moved/changed
    let remainingPieces = Intersection(this.cardAuraPieces, cardAuraPieces, (a,b) => a.id === b.id);
    for(let remainingPiece of remainingPieces){
      //find all affected pieces, compare with affected pieces last process
      let aura = remainingPiece.aura;
      let pieceSelectorParams = {
        selfPiece: remainingPiece,
        controllingPlayerId: remainingPiece.playerId,
        activatingPiece: remainingPiece,
        position: remainingPiece.position,
        isSpell: false
      };
      var selected = this.selector.selectCards(aura.cardSelector, pieceSelectorParams);

      var previouslySelected = this.cardState.cards.filter(p => p.buffs.some(b => b.auraPieceId == remainingPiece.id));

      //add or remove buffs as necessary
      let newComers = Difference(selected, previouslySelected, (a,b) => a.id === b.id);
      this.addBuffs(queue, remainingPiece, newComers, pieceSelectorParams, CardBuff);

      let leavers = Difference(previouslySelected, selected, (a,b) => a.id === b.id);
      this.removeBuffs(queue, remainingPiece, leavers, CardBuff);
    }

    this.cardAuraPieces = cardAuraPieces;
  }

  removeBuffs(queue, auraPiece, affectedPieces, Buff){
    for(let s of affectedPieces){
      let buffToRemove = s.buffs.find(b => b.auraPieceId == auraPiece.id);
      let buff = new Buff(s.id, buffToRemove.name, true);
      queue.pushFront(buff);
    }
  }

  addBuffs(queue, newAuraPiece, selected, pieceSelectorParams, Buff){
    let aura = newAuraPiece.aura;

    for(let s of selected){
      let buff = new Buff(s.id, aura.name, false, newAuraPiece.id);
      for(let buffAttribute of attributes){
        if(aura[buffAttribute]){
          buff[buffAttribute] = this.selector.eventualNumber(aura[buffAttribute], pieceSelectorParams);
        }
      }
      queue.pushFront(buff);
    }
  }
}
