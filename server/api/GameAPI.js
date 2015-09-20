import {route} from 'http-tools';
import {middleware} from 'http-tools';
import authToken from '../middleware/authToken.js';
import roles from '../middleware/roles.js';
import Game from 'models/Game';

@middleware(authToken())
export default class GameAPI
{
  constructor(gameManager, auth)
  {
    this.auth = auth;
    this.manager = gameManager;
  }

  /**
   * Create a new instance of game host
   */
  @route.post('/')
  @middleware(roles(['component']))
  async createGame(req)
  {
    const game = Game.fromJSON(req.body);
    this.manager.create(game);
  }

  /**
   * Shutdown a game
   */
  @route.post('/shutdown')
  @middleware(roles(['component']))
  async shutdown(req)
  {
    const {gameId} = req.body;
    this.manager.shutdown(gameId);
  }
}

