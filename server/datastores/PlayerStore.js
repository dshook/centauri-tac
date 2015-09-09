import Player from 'models/Player';
import loglevel from 'loglevel-decorator';

/**
 * Thrown for logic-level exceptions like bad username or password
 */
export class PlayerStoreError extends Error
{
  constructor(message)
  {
    super();
    this.message = message;
    this.name = 'PlayerStoreError';
  }
}

/**
 * Data layer for players
 */
@loglevel
export default class PlayerStore
{
  constructor(sql, chash)
  {
    this.sql = sql;
    this.chash = chash;
  }

  async getPlayerByEmail(email): ?Player
  {
    const resp = await this.sql.tquery(Player)(`
        select * from players
        where email = @email`, {email});

    return resp.firstOrNull();
  }

  /**
   * Create a new player instance
   */
  async register(email, password): Promise<Player>
  {
    if (await this.getPlayerByEmail(email)) {
      throw new PlayerStoreError('email already registered');
    }

    const hash = await this.chash.hash(password);

    const resp = await this.sql.tquery(Player)(`
        insert into players (email, password)
        values (@email, @hash)
        returning *`, {email, hash});

    const player = resp.firstOrNull();

    this.log.info('registered new player %s', player.email);

    return player;
  }

  /**
   * Check password
   */
  async verify(email, password): Promise<Player>
  {
    const player = this.getPlayerByEmail(email);

    if (!player) {
      throw new PlayerStoreError('email does not exist');
    }

    if (!await this.chash.check(password, player.password)) {
      throw new PlayerStoreError('incorrect password');
    }

    return player;
  }

  /**
   * Obviously needs paging here
   */
  async all(): Promise<Array<Player>>
  {
    const resp = await this.sql.tquery(Player)('select * from players');
    return resp.toArray();
  }
}
