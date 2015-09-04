import moment from 'moment';

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
  static fromSql(data)
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
