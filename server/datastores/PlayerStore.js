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
  constructor(sql, chash, auth, httpTransport)
  {
    this.sql = sql;
    this.chash = chash;
    this.auth = auth;
    this.httpTransport = httpTransport;
  }

  /**
   * Fetch a player by id
   */
  async get(id)
  {
    const resp = await this.sql.tquery(Player)(`
        select * from players
        where id = @id`, {id});

    return resp.firstOrNull();
  }

  /**
   * Get a single player
   */
  async getPlayerByEmail(email)
  {
    const resp = await this.sql.tquery(Player)(`
        select * from players
        where email = @email`, {email});

    return resp.firstOrNull();
  }

  /**
   * Create a new player instance
   */
  async register(email, password)
  {
    if (!email || !email.includes('@')) {
      throw new PlayerStoreError('Valid Email required');
    }
    email = email.toLowerCase();

    if (!password) {
      throw new PlayerStoreError('Password required');
    }
    if (password.length < 8) {
      throw new PlayerStoreError('Password must be at lease 8 characters long');
    }
    if (await this.getPlayerByEmail(email)) {
      throw new PlayerStoreError('Email already registered');
    }

    if(await this.checkPwnedPassword(password)){
      throw new PlayerStoreError('This password has appeard in a data breech. Please use a more secure password.');
    }

    const hash = await this.chash.hash(password);

    const resp = await this.sql.tquery(Player)(`
        insert into players (email, password)
        values (LOWER(@email), @hash)
        returning *`, {email, hash});

    const player = resp.firstOrNull();

    this.log.info('registered new player %s', player.email);

    return player;
  }

  /**
   * Check password
   */
  async verify(email, password)
  {
    if(!email){
      throw new PlayerStoreError('Email Required');
    }
    email = email.toLowerCase();

    const player = await this.getPlayerByEmail(email);

    if (!player) {
      throw new PlayerStoreError('Account not found');
    }

    if (!await this.chash.check(password, player.password)) {
      throw new PlayerStoreError('Incorrect password');
    }

    return player;
  }

  /**
   * Create a JWT for a player
   */
  generateToken(player)
  {
    const roles = ['player'];

    if (player.isAdmin) {
      roles.push('admin');
    }

    return this.auth.generateToken(player, roles);
  }

  //Check if it's a bad pwned password
  async checkPwnedPassword(password){
    const sha1 = this.chash.sha1Hash(password);
    let firstFive = sha1.substring(0, 5).toUpperCase();
    let remaining = sha1.substring(5).toUpperCase();

    this.log.info('Password hashes', {sha1, firstFive, remaining});

    try{
      let httpResponse = await this.httpTransport.request('https://api.pwnedpasswords.com/range/' + firstFive);

      //split into lines, then the hash and count pairs
      let allHashSuffixes = httpResponse.split('\n');
      allHashSuffixes = allHashSuffixes.map(h => h.split(':'));

      let foundHash = allHashSuffixes.find(h => h[0] === remaining);

      //magic number for how many comprimises we tolerate
      return foundHash && parseInt(foundHash[1]) >= 8;
    }catch(e){
      this.log.error('Pwned password API problem %s', e.message);
      //don't return true here so we can still register accounts if the api is down
    }

    return false;
  }
}
