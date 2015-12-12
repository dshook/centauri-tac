import _ from 'lodash';

//Recursive piece selector that takes the selector args from cardlang
export default class PieceSelector{
  constructor(pieces, controllingPlayerId){
    this.allPieces = pieces;
    this.controllingPlayerId = controllingPlayerId;
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
          return leftResult.union(rightResult);
          break;
        case '&':
          return this.Intersection(leftResult, rightResult, (a,b) => a.id == b.id);
          break;
        case '-':
          return leftResult.difference(rightResult);
          break;
      }

    }else{
      return leftResult;
    }

  }

  Intersection(a, b, areEqualFunction){
    var results = [];

    for(var i = 0; i < a.length; i++) {
        var aElement = a[i];

        if(_.any(b, function(bElement) { return areEqualFunction(bElement, aElement); })) {
            results.push(aElement);
        }
    }

    return results;
  }

}