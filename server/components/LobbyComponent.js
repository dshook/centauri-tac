import MatchmakerRPC from '../api/MatchmakerRPC.js';

/**
 * Make me a damn match
 */
export default class LobbyComponent
{
  async start(component)
  {
    const {sockServer} = component;

    sockServer.addHandler(MatchmakerRPC);
  }
}
