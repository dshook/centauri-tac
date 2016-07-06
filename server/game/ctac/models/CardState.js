import _ from 'lodash';
import Random from '../util/Random.js';

/**
 * Current players hands and decks indexed by player id
 */
export default class CardState
{
  constructor()
  {
    this.hands = {};
    this.decks = {};
    this.nextId = 1;
    this.millState = {};
  }

  initPlayer(playerId){
    this.hands[playerId] = [];
    this.decks[playerId] = [];
    this.millState[playerId] = 0;
  }

  addToDeck(playerId, newCard, randomPosition = false){
    let deck = this.decks[playerId];
    newCard.id = this.nextId++;

    if(randomPosition){
      let index = Random.Range(0, deck.length - 1);
      deck.splice(index, 0, newCard);
    }else{
      deck.push(newCard);
    }

    return newCard.id;
  }

  addToHand(playerId, card){
    if(!card.id){
      card.id = this.nextId++;
    }
    this.hands[playerId].push(card);
  }

  //transfer card from deck to hand and return drawn card
  drawCard(playerId){
    let cardDrawn = this.decks[playerId].splice(0, 1)[0];
    this.addToHand(playerId, cardDrawn);
    return cardDrawn;
  }

  validateInHand(playerId, cardId){
    return this.hands[playerId].find(c => c.id === cardId);
  }

  //valdate it's in the hand then remove it
  playCard(playerId, cardId){
    let removed = _.remove(this.hands[playerId], c => c.id === cardId);
    if(removed.length !== 1){
      //something went wrong so return null for an error to be thrown later
      return null;
    }
    return removed[0];
  }
}
