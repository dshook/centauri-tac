import loglevel from 'loglevel-decorator';
import _ from 'lodash';
import PieceSelector from './PieceSelector.js';

@loglevel
export default class Selector{
  constructor(players, pieceState){
    this.players = players;
    this.pieceState = pieceState;
  }

  selectPlayer(controllingPlayerId, selector){
    //for now, selecting a player can only use the single left prop
    if(!selector.left || (selector.left && typeof(selector.left) != 'string' )){
      throw 'Select player can only take basic player selectors';
    }
    switch(selector.left){
      case 'PLAYER':
        return controllingPlayerId;
        break;
      case 'OPPONENT':
        let opponents = this.players.filter(x => x.id !== controllingPlayerId);
        if(opponents.length > 0)
          return opponents[0].id;
        break;
    }
    throw 'Invalid player selector ' + JSON.stringify(selector);
  }

  //select one or more pieces
  selectPieces(controllingPlayerId, selector, triggeringPiece, targetPieceId){
    //for now, only way to get a single piece from a selector is from random
    if(selector.random && selector.selector){
      let selection = this.selectPieces(controllingPlayerId, selector.selector);
      if(selection && selection.length > 0){
        return [_.sample(selection)];
      }
      return [];
    }
    return new PieceSelector(
      this.pieceState.pieces,
      controllingPlayerId,
      triggeringPiece,
      targetPieceId
    ).Select(selector);
  }

  selectPossibleTargets(controllingPlayerId, selector){
    //Random TARGET is not happening
    if(selector.random) return [];

    if(!this.doesSelectorUse(selector, 'TARGET')) return [];

    return new PieceSelector(this.pieceState.pieces, controllingPlayerId)
      .Select(selector);
  }

  //returns t/f if the selctor ever uses the identifier, ex 'TARGET'
  doesSelectorUse(selector, identifier){
    if(selector.random){
      return this.doesSelectorUse(selector.selector, identifier);
    }

    if(selector === identifier || selector.right === identifier){
      return true;
    }
    if(selector.left){
      return this.doesSelectorUse(selector.left, identifier);
    }
    return false;
  }
}

