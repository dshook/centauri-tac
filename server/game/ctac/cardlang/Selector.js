import loglevel from 'loglevel-decorator';
import _ from 'lodash';
import EvalError from './EvalError.js';
import PieceSelector from './PieceSelector.js';
import CardSelector from './CardSelector.js';
import AreaSelector from './AreaSelector.js';

@loglevel
export default class Selector{
  constructor(players, pieceState, mapState, cardState, statsState, playerResourceState, cardDirectory){
    this.players = players;
    this.pieceState = pieceState;
    this.areaSelector = new AreaSelector(this, mapState);
    this.mapState = mapState;
    this.statsState = statsState;
    this.playerResourceState = playerResourceState;
    //For card selector
    this.cardState = cardState;
    this.cardDirectory = cardDirectory;
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
        var possibleTargets = this.selectPossibleTargets(selector, pieceSelectorParams );
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

  //Find tiles on the map that can be cleared and made passable
  selectClearableTiles(selector, pieceSelectorParams){
    this.log.info('Selecting clearable tiles with selector %j', selector);
    //for now, only way to break shit is with AOE's
    if(!selector.left || !selector.left.area){
      return [];
    }

    let areaDescrip = this.selectArea(selector.left, pieceSelectorParams);

    if(areaDescrip.areaTiles.length > 0){
      return this.mapState.map.tiles.filter(p => p.clearable && areaDescrip.areaTiles.some(t => t.tileEquals(p.position)));
    }else{
      return [];
    }
  }

  //similar to select pieces but a more limited set of selections
  //You must specify in the selector if it's a directory or (deck or hand) selection
  //Directory selections match on the template id, while deck and hand selections match on the instance id
  selectCards(selector, cardSelectorParams){
    //for now, only way to get a single card from a selector is from random
    if(selector.random && selector.selector){
      let selection = this.selectCards(selector.selector, cardSelectorParams);
      if(selection && selection.length > 0){
        return [_.sample(selection)];
      }
      return [];
    }
    let isDirectorySelect = this.doesSelectorUse(selector, 'DIRECTORY');
    let isInstanceSelect = this.doesSelectorUse(selector, 'DECK') || this.doesSelectorUse(selector, 'HAND');

    if(!isDirectorySelect && !isInstanceSelect){
      throw 'Card selection must specify directory, deck, or hand';
    }

    return new CardSelector(
      this,
      cardSelectorParams,
      isDirectorySelect
    ).Select(selector);
  }

  selectArea(selector, pieceSelectorParams){
    return this.areaSelector.Select(selector, pieceSelectorParams);
  }

  //Run a piece selection with limited parameters to see what minions could be part of a target selection
  selectPossibleTargets(selector, pieceSelectorParams){
    //Random TARGET is not happening
    if(selector.random) return [];

    if(!this.doesSelectorUse(selector, 'TARGET')) return [];

    let selectionParams = Object.assign({}, pieceSelectorParams);
    //remove the target piece id so we select all possible targets and not just the chosen one
    delete selectionParams.targetPieceId;

    return new PieceSelector(this, selectionParams)
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

    let leftUses = false;
    let rightUses = false;
    if(selector.left){
      leftUses = this.doesSelectorUse(selector.left, identifier);
    }
    if(selector.right){
      rightUses = this.doesSelectorUse(selector.right, identifier);
    }
    return leftUses || rightUses;
  }

  //returns t/f if the selector works with the comparison function
  findSelector(selector, comparison){
    if(selector.random){
      return this.findSelector(selector.selector, comparison);
    }

    if(comparison(selector)){
      return selector;
    }

    let leftResult = false;
    let rightResult = false;
    if(selector.left){
      leftResult = this.findSelector(selector.left, comparison);
    }
    if(selector.right){
      rightResult = this.findSelector(selector.right, comparison);
    }
    return leftResult || rightResult;
  }

  eventualNumber(input, pieceSelectorParams){
    //must be value if the eNumber flag isn't set
    if(!input.eNumber){
      return this.eventualValue(input, pieceSelectorParams);
    }

    switch(input.op){
      case '+':
        return this.eventualNumber(input.left, pieceSelectorParams) + this.eventualNumber(input.right, pieceSelectorParams);
        break;
      case '-':
        return this.eventualNumber(input.left, pieceSelectorParams) - this.eventualNumber(input.right, pieceSelectorParams);
        break;
      case '*':
        return this.eventualNumber(input.left, pieceSelectorParams) * this.eventualNumber(input.right, pieceSelectorParams);
        break;
      case '/':
        return this.eventualNumber(input.left, pieceSelectorParams) / this.eventualNumber(input.right, pieceSelectorParams);
        break;
      case 'negate':
        return -this.eventualNumber(input.left, pieceSelectorParams);
        break;
      default:
        throw 'Invalid eNumber operator ' + input.op;
    }

  }

  //can either be an ordinary number, or something that evaluates to a number
  eventualValue(input, pieceSelectorParams){
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
    }else if(input.cardCount){
      let selectedCards = this.selectCards(input.selector, pieceSelectorParams);
      return selectedCards.length;
    }else if(input.stat){
      return this.statsState.getStat(input.path, pieceSelectorParams.controllingPlayerId);
    }else if(input.resource){
      let selectedPlayer = this.selectPlayer(pieceSelectorParams.controllingPlayerId, input.selector);
      let resourceKey = input.resource;
      return this.playerResourceState.getByPath(resourceKey, selectedPlayer);
    }else if(input.selectCardTemplateId){
      let selectedCards = this.selectCards(input.cardSelector, pieceSelectorParams);
      if(selectedCards.length > 0){
        return selectedCards[0].cardTemplateId;
      }
      return 0;
    }
    return input;
  }

  //Evaluates a compare expression
  //The compare expressions can only have 1 depth so evaluate both the left and right here and return the result
  //compare is also only between two eNumbers, though the attribute selector needs to be evaluated seperately
  //if two attribute selectors are used, they must resolve to 1 piece on each side of the comparison
  //can be coerced to a bool by looking if there were pieces returned or not
  compareExpression(selector, objs, selectorParams, selectFunc){
    let leftResult, rightResult;

    if(selector.left.attributeSelector){
      leftResult = selectFunc.call(this, selector.left.attributeSelector, selectorParams);
    }else{
      leftResult = this.eventualNumber(selector.left, selectorParams);
    }

    if(selector.right.attributeSelector){
      rightResult = selectFunc.call(this, selector.right.attributeSelector, selectorParams);
    }else{
      rightResult = this.eventualNumber(selector.right, selectorParams);
    }

    let leftIsArray = Array.isArray(leftResult);
    let rightIsArray = Array.isArray(rightResult);
    if(leftIsArray && rightIsArray){
      //if either side doesn't have exactly one piece then we can't compare
      if(leftResult.length != 1 || rightResult.length != 1){
        return [];
      }
      //for single pieces on both sides find the attributes and compare
      let lVal = leftResult[0][selector.left.attribute];
      let rVal = rightResult[0][selector.right.attribute];
      let compareResult = this.CompareFromString(lVal, rVal, selector.op);
      if(compareResult){
        return objs;
      }else{
        return [];
      }
    }else if(!leftIsArray && !rightIsArray){
      //two number case
      let compareResult = this.CompareFromString(leftResult, rightResult, selector.op);
      if(compareResult){
        return objs;
      }else{
        return [];
      }
    }else{
      //one number, one piece array case.  Iterate and compare attributes
      let array = leftIsArray ? leftResult : rightResult;
      let number = leftIsArray ? rightResult : leftResult;
      let attribute = leftIsArray ? selector.left.attribute : selector.right.attribute;
      return array.filter(p => this.CompareFromString(p[attribute], number, selector.op));
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

