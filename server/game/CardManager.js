import loglevel from 'loglevel-decorator';

/**
 * Handle player decks and collections
 */
@loglevel
export default class CardManager
{
  constructor(decks)
  {
    this.decks = decks;
  }

  async getDecks(playerId){
    let decks = await this.decks.getDecks(playerId);
    return {
      decks,
      max: 15
    }
  }
}
