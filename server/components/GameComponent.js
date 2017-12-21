import loglevel from 'loglevel-decorator';
import GameRPC from '../api/GameRPC.js';
import CardsAPI from '../api/CardsAPI.js';

/**
 * Game server component that runs game instances
 */
@loglevel
export default class GameComponent
{
  async start(component)
  {
    const {sockServer, restServer} = component;

    sockServer.addHandler(GameRPC);
    restServer.mountController('/cards', CardsAPI);
  }
}
