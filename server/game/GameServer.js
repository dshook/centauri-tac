import loglevel from 'loglevel-decorator';

/**
 * Bootstrap the game server
 */
@loglevel
export default class GameServer
{
  constructor()
  {
    this.log.info('game server started');
  }
}
