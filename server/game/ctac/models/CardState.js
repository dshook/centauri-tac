import _ from 'lodash';

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

  addToDeck(playerId, newCard){
    let deck = this.decks[playerId];
    newCard.id = this.nextId++;

    deck.push(newCard);

    return newCard.id;
  }

  //transfer card from deck to hand and return drawn card
  drawCard(playerId){
    let cardDrawn = this.decks[playerId].splice(0, 1)[0];
    this.hands[playerId].push(cardDrawn);
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
