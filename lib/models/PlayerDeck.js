import fromSQL from 'postgrizzly/fromSQLSymbol';
import {fromInt} from 'models/Races';

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
    g.race = fromInt(resp.race);
    g.type = resp.type; //future use for draft decks etc
    g.isValid = resp.is_valid;
    return g;
  }
}
