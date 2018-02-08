import {Union, Intersection, Difference} from '../util/SetOps.js';

//Recursive card selector that takes the selector args from cardlang
export default class CardSelector{
  constructor(selector, cardSelectorParams, isDirectorySelect){
    this.isDirectorySelect = isDirectorySelect;
    this.inPlayCards = selector.cardState.cards;
    this.directory = Object.keys(selector.cardDirectory.directory).map(k => selector.cardDirectory.directory[k]);

    this.allCards = isDirectorySelect ? this.directory : this.inPlayCards;

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
          return this.allCards.filter(p => p.isMinion);
          break;
        case 'SPELL':
          return this.allCards.filter(p => p.isSpell);
          break;
        case 'HERO':
          return this.allCards.filter(p => p.isHero);
          break;
        case 'MELEE':
          return this.allCards.filter(p => !p.range);
          break;
        case 'RANGED':
          return this.allCards.filter(p => p.range);
          break;
        case 'DECK':
          return this.allCards.filter(p => p.inDeck);
          break;
        case 'HAND':
          return this.allCards.filter(p => p.inHand);
          break;
        case 'DIRECTORY':
          return this.allCards; //Actual directory selection handled in allCards in constructor
          break;
        default:
          throw 'Invalid card type selector ' + selector;
      }
    }

    if(!selector.left){
      throw 'Selector must have left hand side selector';
    }

    //first check if this is a compare expression
    if(selector.compareExpression){
      //throw 'Compare expression not supported for card selection';
      return this.selector.compareExpression(selector, this.allCards, this.cardSelectorParams);
    }

    //ordinary case of recursing the piece selections
    let leftResult = this.Select(selector.left);

    if(selector.op && selector.right){
      let rightResult = this.Select(selector.right);

      let cardEquality = this.isDirectorySelect ?
        (a,b) => a.cardTemplateId === b.cardTemplateId :
        (a,b) => a.id === b.id ;

      switch(selector.op){
        case '|':
          return Union(leftResult, rightResult, cardEquality);
          break;
        case '&':
          return Intersection(leftResult, rightResult, cardEquality);
          break;
        case '-':
          return Difference(leftResult, rightResult, cardEquality);
          break;
      }

    }else{
      return leftResult;
    }
  }
}