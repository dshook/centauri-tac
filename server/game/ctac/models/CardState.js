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

  get cards(){
    let allCards = [];
    for(let playerId in this.hands){
      allCards = allCards.concat(this.hands[playerId]).concat(this.decks[playerId]);
    }
    return allCards;
  }

  card(cardId){
    for(let playerId in this.hands){
      let foundInHand = this.hands[playerId].find(c => c.id === cardId);
      if(foundInHand) return foundInHand;

      let foundInDeck = this.decks[playerId].find(c => c.id === cardId);
      if(foundInDeck) return foundInDeck;
    }
    return null;
  }

  addToDeck(playerId, newCard, randomPosition = false){
    let deck = this.decks[playerId];
    newCard.id = this.nextId++;
    newCard.playerId = playerId; //ensure correct playerId
    newCard.inDeck = true;
    newCard.inHand = false;

    if(randomPosition){
      let index = Random.Range(0, deck.length - 1);
      deck.splice(index, 0, newCard);
    }else{
      deck.push(newCard);
    }

    return newCard.id;
  }

  //Seems like this should have been in sooner than 2 years into the project...
  shuffleDeck(playerId){
    this.decks[playerId] = this.decks[playerId].sort((a, b) => 0.5 - Math.random());
  }

  addToHand(playerId, card){
    if(!card.id){
      card.id = this.nextId++;
    }
    card.playerId = playerId; //ensure correct playerId
    card.inDeck = false;
    card.inHand = true;
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
    removed.inDeck = false;
    removed.inHand = false;
    return removed[0];
  }
}
