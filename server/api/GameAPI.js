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
  constructor(games, auth, componentsConfig)
  {
    this.auth = auth;
    this.games = games;
    this.realm = componentsConfig.realm;
  }

  /**
   * All active games
   */
  @route.get('/')
  @middleware(roles(['player']))
  async getAllGames()
  {
    return await this.games.all(this.realm);
  }

  /**
   * Register a new game (RPC from game component)
   */
  @route.post('/register')
  @middleware(roles(['component']))
  async registerGame(req)
  {
    const game = Game.fromJSON(req.body);
    return await this.games.register(game);
  }

}
