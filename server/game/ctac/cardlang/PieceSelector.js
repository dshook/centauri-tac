import _ from 'lodash';
import Statuses from '../models/Statuses.js';
import loglevel from 'loglevel-decorator';

//Recursive piece selector that takes the selector args from cardlang
export default class PieceSelector{
  constructor(selector, controllingPlayerId, pieceSelectorParams
  ){
    //Include the activating piece in all pieces if it isn't there
    //This comes into affect when activating a piece that's not part of the pieces state yet
    //but should be evaluated as such
    if(pieceSelectorParams.activatingPiece){
      this.allPieces = Array.from(new Set([...selector.pieceState.pieces, pieceSelectorParams.activatingPiece]));
    }else{
      this.allPieces = selector.pieceState.pieces;
    }

    this.mapState = selector.mapState;

    this.controllingPlayerId = controllingPlayerId;
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
      let areaDescrip = this.selector.selectArea(this.controllingPlayerId, selector, this.pieceSelectorParams);

      if(areaDescrip.areaTiles.length > 0){
        return this.allPieces.filter(p => areaDescrip.areaTiles.some(t => t.tileEquals(p.position)));
      }else{
        return [];
      }
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
          return this.Intersection(this.allPieces, this.savedPieces, (a,b) => a.id === b.id);
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
        default:
          throw 'Invalid piece type selector ' + selector;
      }
    }

    if(!selector.left){
      throw 'Selector must have left hand side selector';
    }

    //first check if this is a compare expression
    //The compare expressions can only have 1 depth so evaluate both the left and right here and return the result
    //compare is also only between two eNumbers, though the attribute selector needs to be evaluated seperately
    if(selector.compareExpression){
      let leftResult, rightResult;

      if(selector.left.attributeSelector){
        leftResult = this.selector.selectPieces(this.controllingPlayerId, selector.left.attributeSelector, this.pieceSelectorParams);
      }else{
        leftResult = this.selector.eventualNumber(selector.left, this.controllingPlayerId, this.pieceSelectorParams);
      }

      if(selector.right.attributeSelector){
        rightResult = this.selector.selectPieces(this.controllingPlayerId, selector.right.attributeSelector, this.pieceSelectorParams);
      }else{
        rightResult = this.selector.eventualNumber(selector.right, this.controllingPlayerId, this.pieceSelectorParams);
      }

      let leftIsArray = Array.isArray(leftResult);
      let rightIsArray = Array.isArray(rightResult);
      if(leftIsArray && rightIsArray){
        throw 'Cannot use two attribute selectors in a comparison expression';
      }else if(!leftIsArray && !rightIsArray){
        //two number case
        let compareResult = this.CompareFromString(leftResult, rightResult, selector.op);
        if(compareResult){
          return this.allPieces;
        }else{
          return [];
        }
      }else{
        //one number, one piece array case.  Iterate and compare attributes
        let array = leftIsArray ? leftResult : rightResult;
        let number = leftIsArray ? rightResult : leftResult;
        let attribute = leftIsArray ? selector.left.attribute : selector.right.attribute;
        return this.allPieces.filter(p => this.CompareFromString(p[attribute], number, selector.op));
      }
    }

    //ordinary case of recursing the piece selections
    let leftResult = this.Select(selector.left);

    if(selector.op && selector.right){
      let rightResult = this.Select(selector.right);

      switch(selector.op){
        case '|':
          return this.Union(leftResult, rightResult, (a,b) => a.id === b.id);
          break;
        case '&':
          return this.Intersection(leftResult, rightResult, (a,b) => a.id === b.id);
          break;
        case '-':
          return this.Difference(leftResult, rightResult, (a,b) => a.id === b.id);
          break;
      }

    }else{
      return leftResult;
    }
  }


  Union(a, b, equal){
    var results = [];
    results = results.concat(a);

    for(let bElement of b){
      if(!_.any(results, res => equal(bElement, res) )) {
          results.push(bElement);
      }
    }

    return results;
  }

  Intersection(a, b, equal){
    var results = [];

    for(var i = 0; i < a.length; i++) {
        var aElement = a[i];

        if(_.any(b, bElement => equal(bElement, aElement) )) {
            results.push(aElement);
        }
    }

    return results;
  }

  Difference(a, b, equal){
    var results = [];

    for(var i = 0; i < a.length; i++) {
        var aElement = a[i];

        if(!_.any(b, bElement => equal(bElement, aElement) )) {
            results.push(aElement);
        }
    }

    return results;
  }

  CompareFromString(a, b, op){
    switch(op){
      case '<':
        return a < b;
        break;
      case '>':
        return a > b;
        break;
      case '>=':
        return a >= b;
        break;
      case '<=':
        return a <= b;
        break;
      case '==':
        return a === b;
        break;
      default:
        throw 'Invalid comparison operator ' + op;
    }
  }
}