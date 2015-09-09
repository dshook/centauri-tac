import PlayerAPI from '../api/PlayerAPI.js';

/**
 * Game server component that handles player registration and authentication
 */
export default class AuthComponent
{
  async start(http, rest)
  {
    rest.mountController('/player', PlayerAPI);
  }
}

