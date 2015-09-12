import loglevel from 'loglevel-decorator';
import GameAPI from '../api/GameAPI.js';

/**
 * Game server component that provides lists of running games
 */
@loglevel
export default class GamelistComponent
{
  constructor()
  {

  }

  async start(server, rest)
  {
    rest.mountController('/game', GameAPI);
  }
}
