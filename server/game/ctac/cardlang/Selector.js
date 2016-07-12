import loglevel from 'loglevel-decorator';
import _ from 'lodash';
import EvalError from './EvalError.js';
import PieceSelector from './PieceSelector.js';
import AreaSelector from './AreaSelector.js';

@loglevel
export default class Selector{
  constructor(players, pieceState, mapState){
    this.players = players;
    this.pieceState = pieceState;
    this.areaSelector = new AreaSelector(this, mapState);
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
  selectPieces(selector, pieceSelectorParams){
    //sanity checks first
    if(this.doesSelectorUse(selector, 'TARGET')){

      //skip TARGET checks for timer actions since the target won't be reselected, and SAVED should be used
      if(!pieceSelectorParams.isTimer){
        //make sure that if it's a target card and there are available targets, one of them is picked
        var possibleTargets = this.selectPossibleTargets(
          pieceSelectorParams.controllingPlayerId,
          selector,
          pieceSelectorParams.isSpell
        );
        if(possibleTargets.length > 0 && !possibleTargets.find(p => p.id === pieceSelectorParams.targetPieceId)){
          throw new EvalError('You must select a valid target');
        }

        //if it's a spell (as indicated by no activating piece) and doesn't have any possible targets then reject
        if(pieceSelectorParams.isSpell && possibleTargets.length === 0){
          throw new EvalError('You must select a valid target for this spell');
        }
      }

      //make sure nothing matches target if one isn't provided
      if(!pieceSelectorParams.targetPieceId){
        pieceSelectorParams.targetPieceId = -1;
      }
    }

    //for now, only way to get a single piece from a selector is from random
    if(selector.random && selector.selector){
      let selection = this.selectPieces(selector.selector, pieceSelectorParams);
      if(selection && selection.length > 0){
        return [_.sample(selection)];
      }
      return [];
    }
    return new PieceSelector(
      this,
      pieceSelectorParams
    ).Select(selector);
  }

  selectArea(selector, pieceSelectorParams){
    return this.areaSelector.Select(selector, pieceSelectorParams);
  }

  selectPossibleTargets(controllingPlayerId, selector, isSpell){
    //Random TARGET is not happening
    if(selector.random) return [];

    if(!this.doesSelectorUse(selector, 'TARGET')) return [];

    return new PieceSelector(this, {isSpell, controllingPlayerId})
      .Select(selector);
  }

  //returns t/f if the selector ever uses the identifier, ex 'TARGET'
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

  //returns t/f if the selector works with the comparison function
  findSelector(selector, comparison){
    if(selector.random){
      return this.findSelector(selector.selector, comparison);
    }

    if(comparison(selector) || comparison(selector.right)){
      return selector;
    }
    if(selector.left){
      return this.findSelector(selector.left, comparison);
    }
    return null;
  }

  //can either be an ordinary number, or something that evaluates to a number
  eventualNumber(input, pieceSelectorParams){
    if(input.randList){
      return _.sample(input.randList);
    }else if(input.attributeSelector){
      let selectedPieces = this.selectPieces(input.attributeSelector, pieceSelectorParams);
      if(selectedPieces.length > 0){
        let firstPiece = selectedPieces[0];
        return firstPiece[input.attribute];
      }
      return 0;
    }else if(input.count){
      let selectedPieces = this.selectPieces(input.selector, pieceSelectorParams);
      return selectedPieces.length;
    }
    return input;
  }

  //Evaluates a compare expression
  //The compare expressions can only have 1 depth so evaluate both the left and right here and return the result
  //compare is also only between two eNumbers, though the attribute selector needs to be evaluated seperately
  //can be coerced to a bool by looking if there were pieces returned or not
  compareExpression(selector, pieces, pieceSelectorParams){
    let leftResult, rightResult;

    if(selector.left.attributeSelector){
      leftResult = this.selectPieces(selector.left.attributeSelector, pieceSelectorParams);
    }else{
      leftResult = this.eventualNumber(selector.left, pieceSelectorParams);
    }

    if(selector.right.attributeSelector){
      rightResult = this.selectPieces(selector.right.attributeSelector, pieceSelectorParams);
    }else{
      rightResult = this.eventualNumber(selector.right, pieceSelectorParams);
    }

    let leftIsArray = Array.isArray(leftResult);
    let rightIsArray = Array.isArray(rightResult);
    if(leftIsArray && rightIsArray){
      throw 'Cannot use two attribute selectors in a comparison expression';
    }else if(!leftIsArray && !rightIsArray){
      //two number case
      let compareResult = this.CompareFromString(leftResult, rightResult, selector.op);
      if(compareResult){
        return pieces;
      }else{
        return [];
      }
    }else{
      //one number, one piece array case.  Iterate and compare attributes
      let array = leftIsArray ? leftResult : rightResult;
      let number = leftIsArray ? rightResult : leftResult;
      let attribute = leftIsArray ? selector.left.attribute : selector.right.attribute;
      return pieces.filter(p => this.CompareFromString(p[attribute], number, selector.op));
    }
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

