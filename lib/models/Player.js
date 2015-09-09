import fromSQL from 'postgrizzly/fromSQLSymbol';

/**
 * In game player
 */
export default class Player
{
  constructor()
  {
    this.email = null;
    this.id = null;
    this.registered = null;
  }

  /**
   * From DB
   */
  static [fromSQL](data)
  {
    const p = new Player();
    p.email = data.email;
    p.id = data.id;
    p.registered = data.registered;

    return p;
  }

  toJSON()
  {
    return {
      ...this,
      registered: this.registered.format(),
    };
  }
}
