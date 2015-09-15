import loglevel from 'loglevel-decorator';
import GamelistRPC from '../api/GamelistRPC.js';

/**
 * Game server component that provides lists of running games
 */
@loglevel
export default class GamelistComponent
{
  async start(component)
  {
    const sock = component.sockServer;

    sock.addHandler(GamelistRPC);
  }
}
