import {route} from 'http-tools';
import {HttpError} from 'http-tools';
import {PlayerStoreError} from '../datastores/PlayerStore.js';
import {middleware} from 'http-tools';
import authToken from '../middleware/authToken.js';
import roles from '../middleware/roles.js';

/**
 * REST API for player stuff
 */
@middleware(authToken({ required: false }))
export default class PlayerAPI
{
  constructor(auth, players)
  {
    this.players = players;
    this.auth = auth;
  }

  /**
   * Get list of all players
   */
  @route.get('/')
  async getAll()
  {
    return await this.players.all();
  }

  /**
   * Get the current users profile
   */
  @route.get('/me')
  @middleware(roles(['player']))
  async getPlayerProfile(req)
  {
    return req.auth;
  }

  /**
   * Register an email / username
   */
  @route.post('/register')
  async register(req)
  {
    const {email, password} = req.body;

    try {
      return await this.players.register(email, password);
    }
    catch (err) {

      // Dont swallow other runtime errors
      if (!(err instanceof PlayerStoreError)) {
        throw err;
      }

      // translate player error into transport error
      throw new HttpError(405, err.message);
    }
  }

  /**
   * Get a token for a valid user
   */
  @route.post('/auth')
  async authPlayer(req)
  {
    const {email, password} = req.body;

    let player;

    try {
      player = await this.players.verify(email, password);
    }
    catch (err) {
      if (!(err instanceof PlayerStoreError)) {
        throw err;
      }

      throw new HttpError(405, err.message);
    }

    const token = this.players.generateToken(player);

    return { player, token };
  }
}
