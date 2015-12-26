import _ from 'lodash';

//Recursive piece selector that takes the selector args from cardlang
export default class PieceSelector{
  constructor(pieces, controllingPlayerId, triggeringPiece){
    this.allPieces = pieces;
    this.controllingPlayerId = controllingPlayerId;
    this.triggeringPiece = triggeringPiece;
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
          return this.allPieces.filter(p => p.tags.indexOf('Minion') >= 0);
          break;
        case 'HERO':
          return this.allPieces.filter(p => p.tags.indexOf('Hero') >= 0);
          break;
        case 'SELF':
          if(!this.triggeringPiece) throw 'SELF selector not available without triggering piece';
          return [this.triggeringPiece];
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