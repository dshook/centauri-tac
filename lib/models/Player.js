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
    p.password = data.password;

    return p;
  }

  toJSON()
  {
    const d = {
      ...this,
      registered: this.registered.format(),
    };

    // forreal
    delete d.password;

    return d;
  }
}
