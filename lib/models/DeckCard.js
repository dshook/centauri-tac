import fromSQL from 'postgrizzly/fromSQLSymbol';

/**
 * A deck of cards for a player
 */
export default class DeckCard
{
  constructor()
  {
    this.deckId = null;
    this.cardTemplateId = null;
    this.quantity = null;
  }

  /**
   * From DB
   */
  static [fromSQL](resp)
  {
    if (resp.id === null) {
      return null;
    }

    const g = new DeckCard();
    g.deckId = resp.deck_id;
    g.cardTemplateId = resp.card_template_id;
    g.quantity = resp.quantity;
    return g;
  }

  static fromData(data){
    if (!data) {
      return null;
    }

    const g = new DeckCard();
    Object.assign(g, data);

    return g;
  }
}
