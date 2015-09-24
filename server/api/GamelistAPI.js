import {route} from 'http-tools';
import {middleware} from 'http-tools';
import authToken from '../middleware/authToken.js';
import roles from '../middleware/roles.js';

/**
 * REST API for the gamelist component
 */
@middleware(authToken())
export default class GamelistAPI
{
  constructor(gamelistManager, auth)
  {
    this.auth = auth;
    this.manager = gamelistManager;
  }

  /**
   * Get the current game of a player
   */
  @route.post('/currentGame')
  @middleware(roles(['component']))
  async currentGame(req)
  {
    const {playerId} = req.body;
    const game = await this.manager.getCurrentGame(playerId);
    return {game};
  }

}
