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
          return _.union(leftResult, rightResult);
          break;
        case '&':
          return _.intersection(leftResult, rightResult);
          break;
        case '-':
          return _.difference(leftResult, rightResult);
          break;
      }

    }else{
      return leftResult;
    }

  }

}