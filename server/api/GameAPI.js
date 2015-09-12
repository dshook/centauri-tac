import {route} from 'http-tools';
import {middleware} from 'http-tools';
import authToken from '../middleware/authToken.js';
import roles from '../middleware/roles.js';
import Game from 'models/Game';

/**
 * REST API for dealing with Game models
 */
@middleware(authToken())
export default class GameAPI
{
  constructor(games, auth)
  {
    this.auth = auth;
    this.games = games;
  }

  /**
   * Big list of all games across realms, admin only
   */
  @route.get('/')
  // @middleware(roles(['admin']))
  async getAllGames()
  {
    return await this.games.all();
  }

  /**
   * Register a new game (RPC from game component)
   */
  @route.post('/register')
  async registerGame(req)
  {
    const game = Game.fromJSON(req.body);
    return await this.games.register(game);
  }

}
