import PlayerAPI from '../api/PlayerAPI.js';
import AuthRPC from '../api/AuthRPC.js';

/**
 * Game server component that handles player registration and authentication
 */
export default class AuthComponent
{
  async start(component)
  {
    const rest = component.restServer;
    const sock = component.sockServer;

    rest.mountController('/player', PlayerAPI);

    sock.addHandler(AuthRPC);
  }
}

