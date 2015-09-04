import {route} from 'http-tools';
import {HttpError} from 'http-tools';
import {PlayerStoreError} from '../datastores/PlayerStore.js';

/**
 * REST API for player stuff
 */
export default class PlayerAPI
{
  constructor(players)
  {
    this.players = players;
  }

  @route.get('/')
  async getAll()
  {
    return await this.players.all();
  }

  /**
   * Willy nilly
   */
  @route.post('/register')
  async register(params, req)
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
}
