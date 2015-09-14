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
   * All active games on our realm
   */
  @route.get('/')
  @middleware(roles(['player']))
  async getAllGames()
  {
    return await this.games.all(this.realm);
  }

  @route.get('/:id/players')
  @middleware(roles(['player']))
  async getPlayersInGame(req)
  {
    const {id} = req.params;
    return this.games.playersInGame(id);
  }

  @route.post('/:id/join')
  @middleware(roles(['player']))
  async playerJoin(req)
  {
    // TODO: other logic
    const {playerId} = req.body;
    const gameId = req.params.id;
    return await this.games.playerJoin(playerId, gameId);
  }

  @route.post('/:id/part')
  @middleware(roles(['player']))
  async playerPart(req)
  {
    // TODO: other logic
    const {playerId} = req.body;
    const gameId = req.params.id;
    return await this.games.playerPart(playerId, gameId);
  }


  /**
   * Create a game entry for a player as host
   */
  @route.post('/create')
  @middleware(roles(['player']))
  async registerGame(req)
  {
    const game = Game.fromJSON(req.body);
    return await this.games.register(game);
  }

}
