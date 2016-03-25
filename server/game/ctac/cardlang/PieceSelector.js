import _ from 'lodash';
import Statuses from '../models/Statuses.js';

//Recursive piece selector that takes the selector args from cardlang
export default class PieceSelector{
  constructor(pieces, controllingPlayerId, selfPiece, activatingPiece, targetPieceId){
    //Include the activating piece in all pieces if it isn't there
    //This comes into affect when activating a piece that's not part of the pieces state yet
    //but should be evaluated as such
    if(activatingPiece){
      this.allPieces = Array.from(new Set([...pieces, activatingPiece]));
    }else{
      this.allPieces = pieces;
    }

    this.controllingPlayerId = controllingPlayerId;
    this.selfPiece = selfPiece;
    this.activatingPiece = activatingPiece;
    this.targetPieceId = targetPieceId;
  }

  Select(selector){
    //base case
    if(typeof selector == 'string'){
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
          return this.allPieces.filter(p =>
            p.baseTags.length === 1
            && (p.baseTags.includes('Hero') || p.baseTags.includes('Minion'))
            && p.baseStatuses === 0
          );
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
          if(!this.targetPieceId) return this.allPieces;
          return [{id: this.targetPieceId}];
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
        case 'ROOTED':
          return this.allPieces.filter(p => p.statuses & Statuses.Rooted);
          break;
        default:
          throw 'Invalid piece type selector ' + selector;
      }
    }

    if(!selector.left){
      throw 'Selector must have left hand side selector';
    }

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
}