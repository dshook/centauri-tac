import loglevel from 'loglevel-decorator';
import PlayerDeck from 'models/PlayerDeck';
import DeckCards from 'models/DeckCards';
import hrtime from 'hrtime-log-decorator';

/**
 * Data layer for games
 */
@loglevel
export default class DeckStore
{
  constructor(sql)
  {
    this.sql = sql;
  }

  /**
   * All player decks
   */
  @hrtime('fetched decks %s ms')
  async getDecks(playerId)
  {
    let sql = `
      select *
      from player_decks
      where player_id = @playerId
    `;

    const resp = await this.sql.tquery(PlayerDeck)(sql, {playerId});

    let decks = resp.toArray();
    let deckResolves = [];

    //make promises to fetch all the cards in each deck
    for(let deck of decks){
      deckResolves.push(
        this.getDeckCards(deck.id)
          .then(deckCards => deck.cards = deckCards)
      );
    }

    return Promise.all(deckResolves);
  }

  async getDeckCards(deckId)
  {
    let sql = `
      select *
      from deck_cards
      where deck_id = @deckId
    `;

    const resp = await this.sql.tquery(DeckCards)(sql, {deckId});

    return resp.toArray();
  }

}
