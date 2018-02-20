import {rpc} from 'sock-harness';
import {PlayerStoreError} from '../datastores/PlayerStore.js';
import loglevel from 'loglevel-decorator';
import roles from '../middleware/rpc/roles.js';

/**
 * RPC handler for the auth component
 */
@loglevel
export default class AuthRPC
{
  constructor(auth, players)
  {
    this.auth = auth;
    this.players = players;
  }

  /**
   * Post creds and get back a token if it works
   */
  @rpc.command('register')
  async register(client, {email, password})
  {
    try {
      await this.players.register(email, password);
    }
    catch (err) {
      if (!(err instanceof PlayerStoreError)) {
        throw err;
      }

      // Bad email or password
      client.send('register', {status: false, message: err.message});
      client.token = null;
      return;
    }

    this.log.info('registered new player %s', email);

    const message = 'Registration Successful';
    client.send('register', {status: true, message});
  }

  /**
   * Post creds and get back a token if it works
   */
  @rpc.command('login')
  async login(client, {email, password})
  {
    let player;

    try {
      player = await this.players.verify(email, password);
    }
    catch (err) {
      if (!(err instanceof PlayerStoreError)) {
        throw err;
      }

      // Bad email or password
      client.send('login', {status: false, message: err.message});
      client.token = null;
      return;
    }

    this.log.info('auth player %s posted valid creds, sending token', email);

    // update remote's token and stash here
    const token = this.players.generateToken(player);
    client.send('token', token);
    client.token = token;

    // Update remotes login state
    const message = 'login successful';
    client.send('login', {status: true, message});
  }

  /**
   * Send back a players profile
   */
  @rpc.command('me')
  @rpc.middleware(roles(['player']))
  async profile(client, params, auth)
  {
    if(!auth || !auth.sub){
      this.log.warn('client not logged in for me command');
      return;
    }
    const {id} = auth.sub;
    let player = await this.players.get(id);

    this.log.info('Sending me for %s', player.email);
    client.send('me', player);
  }
}
