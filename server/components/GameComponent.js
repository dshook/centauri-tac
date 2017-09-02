import loglevel from 'loglevel-decorator';
import GameRPC from '../api/GameRPC.js';

/**
 * Game server component that runs game instances
 */
@loglevel
export default class GameComponent
{
  async start(component)
  {
    const {sockServer} = component;

    sockServer.addHandler(GameRPC);
  }
}
