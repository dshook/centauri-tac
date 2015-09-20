import loglevel from 'loglevel-decorator';

/**
 * Top-level entity for a running game
 */
@loglevel
export default class GameHost
{
  constructor(game, instance)
  {
    this.log.info('created new GameHost for game %s', game.id);
    this.game = game;
    this.instance = instance;
    this.clients = [];
  }

  async shutdown()
  {
    this.log.info('shutting down!');
  }
}
