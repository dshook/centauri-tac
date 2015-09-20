import {route} from 'http-tools';
import {middleware} from 'http-tools';
import authToken from '../middleware/authToken.js';
import roles from '../middleware/roles.js';
import Game from 'models/Game';
import loglevel from 'loglevel-decorator';

@loglevel
@middleware(authToken())
export default class GameAPI
{
  constructor(netClient, auth)
  {
    this.net = netClient;
    this.auth = auth;
  }

  /**
   * Create a new instance of game host
   */
  @route.post('/')
  @middleware(roles(['component']))
  async createGame(req)
  {
    const game = Game.fromJSON(req.body);
    this.log.info('TODO: instantiate new game %s %s for gamelist',
        game.id, game.name);

    // inform server our state has changed to staging
    const gameId = game.id;
    const stateId = 2;
    await this.net.sendCommand('gamelist', 'update:state', {gameId, stateId});
  }

  /**
   * Create a new instance of game host
   */
  @route.delete('/shutdown')
  @middleware(roles(['component']))
  async shutdown(req)
  {
    const {gameId} = req.body;
    this.log.info('TODO: shut down game %s for gamelist', gameId);
  }
}

