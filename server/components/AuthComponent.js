import loglevel from 'loglevel-decorator';
import PlayerAPI from '../api/PlayerAPI.js';

@loglevel
export default class AuthComponent
{
  async start(http, rest)
  {
    rest.mountController('/player', PlayerAPI);
  }
}

