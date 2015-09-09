import loglevel from 'loglevel-decorator';
import PlayerAPI from '../api/PlayerAPI.js';

@loglevel
export default class AuthComponent
{
  constructor(rest, players)
  {
    this.players = players;
    this.rest = rest;
  }

  async start()
  {
    this.rest.mountController('/player', PlayerAPI);
  }
}

