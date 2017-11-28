import fromSQL from 'postgrizzly/fromSQLSymbol';
import DeckCard from 'models/DeckCard';

/**
 * A deck of cards for a player
 */
export default class PlayerDeck
{
  constructor()
  {
    this.id = null;
    this.playerId = null;
    this.name = null;
    this.race = null;
    this.type = null;
    this.isValid = false;

    //array of DeckCards
    this.cards = null;
  }

  cardCount(){
    if(!this.cards || this.cards.length === 0){
      return 0;
    }

    return this.cards.reduce((prev, next) => prev + Math.abs(next.quantity), 0)
  }

  /**
   * From DB
   */
  static [fromSQL](resp)
  {
    if (resp.id === null) {
      return null;
    }

    const g = new PlayerDeck();
    g.id = resp.id;
    g.playerId = resp.player_id;
    g.name = resp.name;
    g.race = resp.race;
    g.type = resp.type; //future use for draft decks etc
    g.isValid = resp.is_valid;
    return g;
  }

  /**
   * From rpc
   */
  static fromData(data)
  {
    if (!data) {
      return null;
    }

    const g = new PlayerDeck();
    Object.assign(g, data);

    g.cards = (data.cards && data.cards.length)
      ? data.cards.map(c => DeckCard.fromData(c))
      : [];

    g.type = g.type || 0;

    return g;
  }
}
