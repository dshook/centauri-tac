import loglevel from 'loglevel-decorator';
import DeckStoreError from 'errors/DeckStoreError';
import Constants from './ctac/util/Constants.js';
import Rarities from './ctac/models/Rarities';

/**
 * Handle player decks and collections
 */
@loglevel
export default class CardManager
{
  constructor(decks, cardDirectory)
  {
    this.decks = decks;
    this.cardDirectory = cardDirectory;
  }

  async getDecks(playerId){
    let decks = await this.decks.getDecks(playerId);
    return {
      decks,
      max: 15
    }
  }

  async saveDeck(playerId, deck){
    if(!deck){
      throw new DeckStoreError('Missing deck to save');
    }
    if(!playerId){
      throw new DeckStoreError('Missing player to save deck for');
    }

    //make sure we're saving for the logged in player and they're not trying to spoof us
    deck.playerId = playerId;

    if(deck.cards.find(c => c.quantity < 0)){
      throw new DeckStoreError('Cannot have negative quantity cards');
    }

    if(deck.cardCount() > Constants.deckCardLimit){
      throw new DeckStoreError('Deck has too many cards');
    }

    if(deck.cards.find(c => !this.cardDirectory.directory[c.cardTemplateId])){
      throw new DeckStoreError('Trying to add unknown card');
    }

    if(deck.cards.find(c => c.quantity > 2) ){
      throw new DeckStoreError('Only 2 copies of a card are allowed in your deck');
    }

    if(deck.cards.find(c =>
      this.cardDirectory.directory[c.cardTemplateId].rarity === Rarities.Ascendant
      && c.quantity > 1)
    ){
      throw new DeckStoreError('Only 1 of each Ascendant card is allowed in your deck');
    }

    deck.isValid = deck.cardCount() === 30;

    return await this.decks.upsertDeck(deck);
  }

  async deleteDeck(playerId, deckId){
    return await this.decks.deleteDeck(playerId, deckId);
  }
}
