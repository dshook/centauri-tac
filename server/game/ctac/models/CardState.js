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
  }

  initPlayer(playerId){
    this.hands[playerId] = [];
    this.decks[playerId] = [];
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

  //valdate it's in the hand then remove it
  playCard(playerId, cardId){
    let removed = _.remove(this.hands[playerId], c => c.id === cardId);
    return removed.length;
  }
}