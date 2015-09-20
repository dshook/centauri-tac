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
  constructor(auth)
  {
    this.auth = auth;
  }

  /**
   * Create a new instance of game host
   */
  @route.post('')
  @middleware(roles(['component']))
  createGame(req)
  {
    const game = Game.fromJSON(req.body);
    this.log.info('TODO: instantiate new game %s %s', game.id, game.name);
  }
}

