import Statuses from '../models/Statuses.js';
import loglevel from 'loglevel-decorator';
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

    // 'optional' params that are only used in some selectors
    this.selfPiece = pieceSelectorParams.selfPiece;
    this.activatingPiece = pieceSelectorParams.activatingPiece;
    this.targetPieceId = pieceSelectorParams.targetPieceId;
    this.savedPieces = pieceSelectorParams.savedPieces;
    this.isSpell = pieceSelectorParams.isSpell;
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

    if(selector.id){
      return this.allPieces.filter(p => p.cardTemplateId == selector.id);
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
          return this.allPieces.filter(p => p.tags.includes('Minion'));
          break;
        case 'HERO':
          return this.allPieces.filter(p => p.tags.includes('Hero'));
          break;
        case 'BASIC':
          let basicBitches = this.allPieces.filter(p =>
            p.baseTags.length === 1
            && (p.baseTags.includes('Hero') || p.baseTags.includes('Minion'))
            && p.baseStatuses === 0
          );
          return basicBitches;
          break;
        case 'DAMAGED':
          return this.allPieces.filter(p => p.health < p.baseHealth);
          break;
        case 'ACTIVATOR':
          if(!this.activatingPiece) return [];
          return [this.activatingPiece];
          break;
        case 'SELF':
          if(!this.selfPiece) return [];
          return [this.selfPiece];
          break;
        case 'TARGET':
          return this.allPieces.filter(p =>
            (!this.targetPieceId || p.id === this.targetPieceId)
            && !(p.statuses & Statuses.Cloak)
            && (!this.isSpell || !(p.statuses & Statuses.TechResist))
          );
          break;
        case 'SAVED':
          if(!this.savedPieces) return [];
          //Use an implicit intersection with all pieces in case one of the saved pieces is now gone
          return Intersection(this.allPieces, this.savedPieces, (a,b) => a.id === b.id);
          break;
        case 'SILENCE':
          return this.allPieces.filter(p => p.statuses & Statuses.Silence);
          break;
        case 'SHIELD':
          return this.allPieces.filter(p => p.statuses & Statuses.Shield);
          break;
        case 'PARALYZE':
          return this.allPieces.filter(p => p.statuses & Statuses.Paralyze);
          break;
        case 'TAUNT':
          return this.allPieces.filter(p => p.statuses & Statuses.Taunt);
          break;
        case 'CLOAK':
          return this.allPieces.filter(p => p.statuses & Statuses.Cloak);
          break;
        case 'TECHRESIST':
          return this.allPieces.filter(p => p.statuses & Statuses.TechResist);
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
        default:
          throw 'Invalid piece type selector ' + selector;
      }
    }

    if(!selector.left){
      throw 'Selector must have left hand side selector';
    }

    //first check if this is a compare expression
    if(selector.compareExpression){
      return this.selector.compareExpression(selector, this.allPieces, this.pieceSelectorParams);
    }

    //ordinary case of recursing the piece selections
    let leftResult = this.Select(selector.left);

    if(selector.op && selector.right){
      let rightResult = this.Select(selector.right);

      switch(selector.op){
        case '|':
          return Union(leftResult, rightResult, (a,b) => a.id === b.id);
          break;
        case '&':
          return Intersection(leftResult, rightResult, (a,b) => a.id === b.id);
          break;
        case '-':
          return Difference(leftResult, rightResult, (a,b) => a.id === b.id);
          break;
      }

    }else{
      return leftResult;
    }
  }
}