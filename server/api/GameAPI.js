import {route} from 'http-tools';
import {middleware} from 'http-tools';
import authToken from '../middleware/authToken.js';
import roles from '../middleware/roles.js';

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
   * All active games on our realm
   */
  @route.get('/')
  @middleware(roles(['player']))
  async getAllGames()
  {
    return await this.games.all(this.realm);
  }

  /**
   * Get a players current game (or null)
   */
  @route.get('/current')
  @middleware(roles(['player']))
  async getPlayersCurrentGame(req)
  {
    const id = req.auth.sub.id;
    return await this.games.playersCurrentGame(id);
  }

  /**
   * Player list of who is currently in a game.
   */
  @route.get('/:id/players')
  @middleware(roles(['player']))
  async getPlayersInGame(req)
  {
    const {id} = req.params;
    return this.games.playersInGame(id);
  }

  /**
   * Player asking to join game
   */
  @route.post('/:id/join')
  @middleware(roles(['player']))
  async playerJoin(req)
  {
    const playerId = req.auth.sub.id;
    const gameId = req.params.id;

    // TODO: interact with the running game

    return await this.games.playerJoin(playerId, gameId);
  }

  /**
   * Player asking to leave game
   */
  @route.post('/:id/part')
  @middleware(roles(['player']))
  async playerPart(req)
  {
    const playerId = req.auth.sub.id;
    const gameId = req.params.id;

    // TODO: interact with the running game

    return await this.games.playerPart(playerId, gameId);
  }
}
