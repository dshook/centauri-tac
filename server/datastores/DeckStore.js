import loglevel from 'loglevel-decorator';
import PlayerDeck from 'models/PlayerDeck';
import DeckCard from 'models/DeckCard';
import hrtime from 'hrtime-log-decorator';
import DeckStoreError from 'errors/DeckStoreError';

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

    await Promise.all(deckResolves);
    return decks;
  }

  async getDeckCards(deckId)
  {
    let sql = `
      select *
      from deck_cards
      where deck_id = @deckId
    `;

    const resp = await this.sql.tquery(DeckCard)(sql, {deckId});

    return resp.toArray();
  }

  //Update or insert a deck
  //TODO: might need a transaction at some point?
  async upsertDeck(deck){
    let insertSql = `
      insert into player_decks (player_id, name, race, type, is_valid)
        values (@playerId, @name, @race, @type, @isValid)
      returning id;
    `;

    let upsertSql = `
      insert into player_decks (id, player_id, name, race, type, is_valid)
        values (@id, @playerId, @name, @race, @type, @isValid)
      on conflict (id)
      do update set (name, race, type, is_valid) = (@name, @race, @type, @isValid)
      returning id;
    `;

    let resp = await this.sql.query(deck.id ? upsertSql : insertSql, deck)
    let {id} = resp.firstOrNull();

    await this.sql.query('delete from deck_cards where deck_id = @id', {id});

    //make sure the deck cards have the right id now that it may have been created
    for(let card of deck.cards){
      card.deckId = id;
    }

    await Promise.map(deck.cards, card => this.sql.query(`
        INSERT INTO deck_cards (deck_id, card_template_id, quantity)
        VALUES (@deckId, @cardTemplateId, @quantity);
      `, card)
    );

    deck.id = id;

    return deck;
  }

  async deleteDeck(playerId, deckId){
    let sql = `
      select *
      from player_decks
      where id = @deckId and player_id = @playerId
    `;

    const resp = await this.sql.tquery(PlayerDeck)(sql, {deckId, playerId});

    if(!resp){
      this.log.warn('No deck found to delete with id %s player %s', deckId, playerId)
      throw new DeckStoreError('Deck not found to delete');
    }

    await this.sql.query(`
      delete from deck_cards
      where deck_id = @deckId
    ` , {deckId});

    await this.sql.query(`
      delete from player_decks
      where id = @deckId and player_id = @playerId
    ` , {deckId, playerId});

    return true;
  }

}
