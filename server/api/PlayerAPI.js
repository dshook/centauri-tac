import {route} from 'http-tools';
import {HttpError} from 'http-tools';
import {PlayerStoreError} from '../datastores/PlayerStore.js';

/**
 * REST API for player stuff
 */
export default class PlayerAPI
{
  constructor(auth, players)
  {
    this.players = players;
    this.auth = auth;
  }

  /**
   * Register an email / username (public)
   */
  @route.post('/register')
  async register(req)
  {
    const {email, password} = req.body;

    try {
      return await this.players.register(email, password);
    }
    catch (err) {
      if (!(err instanceof PlayerStoreError)) {
        throw err;
      }

      // invalid registration
      throw new HttpError(400, err.message);
    }
  }

  /**
   * Get a token for a valid user (public)
   */
  @route.post('/login')
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

      // Bad email or password
      throw new HttpError(401, err.message);
    }

    const token = this.players.generateToken(player);

    return {token};
  }
}

