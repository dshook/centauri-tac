import GamePiece from '../models/GamePiece.js';
import PieceBuff from '../actions/PieceBuff.js';
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
  constructor(pieceState, cardEvaluator, selector)
  {
    this.pieceState = pieceState;
    this.cardEvaluator = cardEvaluator;
    this.selector = selector;

    //keep track of which pieces we've already 'activated'
    this.auraPieces = [];
  }

  /**
   * Proc
   */
  async handleAction(action, queue)
  {
    if (action != null && !(action instanceof UpdateAura)) {
      return;
    }

    let auraPieces = this.pieceState.pieces.filter(p => p.aura != null);

    //find out newly added pieces
    let newlyAdded = Difference(auraPieces, this.auraPieces, (a,b) => a.id === b.id);

    //add buffs to all selected pieces
    for(let newAuraPiece of newlyAdded){
      let aura = newAuraPiece.aura;
      let pieceSelectorParams = {
        selfPiece: newAuraPiece,
        activatingPiece: newAuraPiece,
        position: newAuraPiece.position,
        isSpell: false
      };
      var selected = this.selector.selectPieces(newAuraPiece.playerId, aura.pieceSelector, pieceSelectorParams);
      //set up a new buff for each selected piece that has all the attributes of the buff
      this.addBuffs(queue, newAuraPiece, selected, pieceSelectorParams);
    }

    //find removed pieces
    let removedPieces = Difference(this.auraPieces, auraPieces, (a,b) => a.id === b.id);
    for(let oldAuraPiece of removedPieces){
      let aura = oldAuraPiece.aura;
      let affectedPieces = this.pieceState.pieces.filter(p => p.buffs.some(b => b.auraPieceId == oldAuraPiece.id));
      //remove buffs from all piece that had a buff from that piece
      this.removeBuffs(queue, oldAuraPiece, affectedPieces);
    }

    //find remaining pieces that still might have moved/changed
    let remainingPieces = Intersection(this.auraPieces, auraPieces, (a,b) => a.id === b.id);
    for(let remainingPiece of remainingPieces){
      //find all affected pieces, compare with affected pieces last process
      let aura = remainingPiece.aura;
      let pieceSelectorParams = {
        selfPiece: remainingPiece,
        activatingPiece: remainingPiece,
        position: remainingPiece.position,
        isSpell: false
      };
      var selected = this.selector.selectPieces(remainingPiece.playerId, aura.pieceSelector, pieceSelectorParams);

      var previouslySelected = this.pieceState.pieces.filter(p => p.buffs.some(b => b.auraPieceId == remainingPiece.id));

      //add or remove buffs as necessary
      let newComers = Difference(selected, previouslySelected, (a,b) => a.id === b.id);
      this.addBuffs(queue, remainingPiece, newComers, pieceSelectorParams);

      let leavers = Difference(previouslySelected, selected, (a,b) => a.id === b.id);
      this.removeBuffs(queue, remainingPiece, leavers);
    }

    this.auraPieces = auraPieces;

    this.log.info('Updated auras');
  }

  removeBuffs(queue, auraPiece, affectedPieces){
    for(let s of affectedPieces){
      let buffToRemove = s.buffs.find(b => b.auraPieceId == auraPiece.id);
      let buff = new PieceBuff(s.id, buffToRemove.name, true);
      queue.pushFront(buff);
    }
  }

  addBuffs(queue, newAuraPiece, selected, pieceSelectorParams){
    let aura = newAuraPiece.aura;

    for(let s of selected){
      let buff = new PieceBuff(s.id, aura.name, false, newAuraPiece.id);
      for(let buffAttribute of attributes){
        if(aura[buffAttribute]){
          buff[buffAttribute] = this.selector.eventualNumber(aura[buffAttribute], newAuraPiece.playerId, pieceSelectorParams);
        }
      }
      queue.pushFront(buff);
    }
  }
}
