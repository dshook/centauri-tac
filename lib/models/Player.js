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
    this.isAdmin = null;
    this.password = null;

    // client-side state stuff
    this.connected = null;
    this.client = null;
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
    p.isAdmin = data['is_admin'];

    return p;
  }

  /**
   * From the auth payload on a token via a client
   */
  static fromClient(client)
  {
    const {auth} = client;

    const p = new Player();

    p.id = auth.sub.id;
    p.email = auth.sub.email;
    p.isAdmin = auth.roles.some(x => x === 'admin');
    p.client = client;

    return p;
  }

  /**
   * Format output
   */
  toJSON()
  {
    const d = {
      ...this,
      registered: this.registered ? this.registered.format() : null,
    };

    // forreal
    delete d.password;
    delete d.client;

    return d;
  }
}
