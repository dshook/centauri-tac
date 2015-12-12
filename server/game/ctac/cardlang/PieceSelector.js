import _ from 'lodash';

//Recursive piece selector that takes the selector args from cardlang
export default class PieceSelector{
  constructor(pieces, controllingPlayerId){
    this.allPieces = _.chain(pieces);
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

    //console.log('left', leftResult.value());

    if(selector.op && selector.right){
      let rightResult = this.Select(selector.right);
      //console.log('right', rightResult.value());
      //TODO: Seemingly the array references coming back from left and right
      //aren't matching and therefore interesction isn't matching any results
      //need to investigate more

      switch(selector.op){
        case '|':
          return leftResult.union(rightResult);
          break;
        case '&':
          return leftResult.intersection(rightResult);
          break;
        case '-':
          return leftResult.difference(rightResult);
          break;
      }

    }else{
      return leftResult;
    }

  }

}