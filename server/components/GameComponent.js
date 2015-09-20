import loglevel from 'loglevel-decorator';
import GameRPC from '../api/GameRPC.js';
import GameAPI from '../api/GameAPI.js';

/**
 * Game server component that runs game instances
 */
@loglevel
export default class GameComponent
{
  async start(component)
  {
    const {sockServer} = component;
    const {restServer} = component;

    sockServer.addHandler(GameRPC);
    restServer.mountController('/game', GameAPI);
  }
}
