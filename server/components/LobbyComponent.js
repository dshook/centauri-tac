import LobbyRPC from '../api/LobbyRPC.js';

/**
 * Make me a damn match
 */
export default class LobbyComponent
{
  async start(component)
  {
    const {sockServer} = component;

    sockServer.addHandler(LobbyRPC);
  }
}
