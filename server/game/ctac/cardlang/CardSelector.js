import loglevel from 'loglevel-decorator';
import {Union, Intersection, Difference} from '../util/SetOps.js';

//Recursive card selector that takes the selector args from cardlang
export default class CardSelector{
  constructor(selector, cardSelectorParams){
    this.allCards = selector.cardState.cards;

    this.controllingPlayerId = cardSelectorParams.controllingPlayerId;
    this.selector = selector;
    this.cardSelectorParams = cardSelectorParams;
  }

  Select(selector){

    if(selector.tag){
      return this.allCards.filter(p => p.tags.includes(selector.tag));
    }

    if(selector.id){
      return this.allCards.filter(p => p.cardTemplateId == selector.id);
    }

    //base case
    else if(typeof selector == 'string'){
      switch(selector){
        case 'FRIENDLY':
          return this.allCards.filter(p => p.playerId == this.controllingPlayerId);
          break;
        case 'ENEMY':
          return this.allCards.filter(p => p.playerId != this.controllingPlayerId);
          break;
        case 'MINION':
          return this.allCards.filter(p => p.tags.includes('Minion'));
          break;
        case 'SPELL':
          return this.allCards.filter(p => p.tags.includes('Spell'));
          break;
        case 'DECK':
          return this.allCards.filter(p => p.inDeck);
          break;
        case 'HAND':
          return this.allCards.filter(p => p.inHand);
          break;
      }
    }

    if(!selector.left){
      throw 'Selector must have left hand side selector';
    }

    //first check if this is a compare expression
    if(selector.compareExpression){
      throw 'Compare expression not supported for card selection';
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