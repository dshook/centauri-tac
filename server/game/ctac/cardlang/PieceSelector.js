import Statuses from '../models/Statuses.js';
import {Union, Intersection, Difference} from '../util/SetOps.js';

//Recursive piece selector that takes the selector args from cardlang
export default class PieceSelector{
  constructor(selector, pieceSelectorParams){
    //Include the activating piece in all pieces if it isn't there
    //This comes into affect when activating a piece that's not part of the pieces state yet
    //but should be evaluated as such
    if(pieceSelectorParams.activatingPiece){
      this.allPieces = Array.from(new Set([...selector.pieceState.pieces, pieceSelectorParams.activatingPiece]));
    }else{
      this.allPieces = selector.pieceState.pieces;
    }

    this.mapState = selector.mapState;

    this.controllingPlayerId = pieceSelectorParams.controllingPlayerId;
    this.selector = selector;
    this.pieceSelectorParams = pieceSelectorParams;
  }

  Select(selector){
    //area case
    //first find the centering piece then all the pieces in the area
    if(selector.area){
      let areaDescrip = this.selector.selectArea(selector, this.pieceSelectorParams);

      if(areaDescrip.areaTiles.length > 0){
        return this.allPieces.filter(p => areaDescrip.areaTiles.some(t => t.tileEquals(p.position)));
      }else{
        return [];
      }
    }

    if(selector.tag){
      return this.allPieces.filter(p => p.tags.includes(selector.tag));
    }

    if(selector.templateId){
      return this.allPieces.filter(p => p.cardTemplateId == selector.templateId);
    }

    if(selector.pieceIds){
      return this.allPieces.filter(p => selector.pieceIds.includes(p.id));
    }

    //base case
    else if(typeof selector == 'string'){
      switch(selector){
        case 'CHARACTER':
          return this.allPieces;
          break;
        case 'FRIENDLY':
          return this.allPieces.filter(p => p.playerId == this.controllingPlayerId);
          break;
        case 'ENEMY':
          return this.allPieces.filter(p => p.playerId != this.controllingPlayerId);
          break;
        case 'MINION':
          return this.allPieces.filter(p => p.isMinion);
          break;
        case 'HERO':
          return this.allPieces.filter(p => p.isHero);
          break;
        case 'BASIC':
          return this.allPieces.filter(p => !p.description);
          break;
        case 'DAMAGED':
          return this.allPieces.filter(p => p.health < p.maxBuffedHealth);
          break;
        case 'MELEE':
          return this.allPieces.filter(p => !p.range);
          break;
        case 'RANGED':
          return this.allPieces.filter(p => p.range);
          break;
        case 'ACTIVATOR':
          if(!this.pieceSelectorParams.activatingPiece) return [];
          return [this.pieceSelectorParams.activatingPiece];
          break;
        case 'SELF':
          if(!this.pieceSelectorParams.selfPiece) return [];
          return [this.pieceSelectorParams.selfPiece];
          break;
        case 'SELECTED':
          if(!this.pieceSelectorParams.selectedPiece) return [];
          return [this.pieceSelectorParams.selectedPiece];
          break;
        case 'TARGET':
          return this.allPieces.filter(p =>
            (!this.pieceSelectorParams.targetPieceId || p.id === this.pieceSelectorParams.targetPieceId)
            && !(p.statuses & Statuses.Cloak)
            && (!this.pieceSelectorParams.isSpell || !(p.statuses & Statuses.Elusive))
            && (!this.pieceSelectorParams.selfPiece || p.id !== this.pieceSelectorParams.selfPiece.id) //can't target yourself
          );
          break;
        case 'SAVED':
          if(!this.pieceSelectorParams.savedPieces) return [];
          //Use an implicit intersection with all pieces in case one of the saved pieces is now gone
          return Intersection(this.allPieces, this.pieceSelectorParams.savedPieces, (a,b) => a.id === b);
          break;
        case 'SILENCE':
          return this.allPieces.filter(p => p.statuses & Statuses.Silence);
          break;
        case 'SHIELD':
          return this.allPieces.filter(p => p.statuses & Statuses.Shield);
          break;
        case 'PETRIFY':
          return this.allPieces.filter(p => p.statuses & Statuses.Petrify);
          break;
        case 'TAUNT':
          return this.allPieces.filter(p => p.statuses & Statuses.Taunt);
          break;
        case 'CLOAK':
          return this.allPieces.filter(p => p.statuses & Statuses.Cloak);
          break;
        case 'ELUSIVE':
          return this.allPieces.filter(p => p.statuses & Statuses.Elusive);
          break;
        case 'ROOT':
          return this.allPieces.filter(p => p.statuses & Statuses.Rooted);
          break;
        case 'CANTATTACK':
          return this.allPieces.filter(p => p.statuses & Statuses.CantAttack);
          break;
        case 'DYADSTRIKE':
          return this.allPieces.filter(p => p.statuses & Statuses.DyadStrike);
          break;
        case 'FLYING':
          return this.allPieces.filter(p => p.statuses & Statuses.Flying);
          break;
        case 'AIRDROP':
          return this.allPieces.filter(p => p.statuses & Statuses.Airdrop);
          break;
        case 'CLEAVE':
          return this.allPieces.filter(p => p.statuses & Statuses.Cleave);
          break;
        case 'PIERCING':
          return this.allPieces.filter(p => p.statuses & Statuses.Piercing);
          break;
        default:
          throw 'Invalid piece type selector ' + selector;
      }
    }

    if(!selector.left){
      throw 'Selector must have left hand side selector';
    }

    //first check if this is a compare expression
    if(selector.compareExpression){
      return this.selector.compareExpression(selector, this.allPieces, this.pieceSelectorParams, this.selector.selectPieces);
    }

    //ordinary case of recursing the piece selections
    let leftResult = this.Select(selector.left);

    if(selector.op && selector.right){
      let rightResult = this.Select(selector.right);
      let pieceEquality = (a,b) => a.id === b.id;

      switch(selector.op){
        case '|':
          return Union(leftResult, rightResult, pieceEquality);
          break;
        case '&':
          return Intersection(leftResult, rightResult, pieceEquality);
          break;
        case '-':
          return Difference(leftResult, rightResult, pieceEquality);
          break;
      }

    }else{
      return leftResult;
    }
  }
}