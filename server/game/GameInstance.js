import loglevel from 'loglevel-decorator';

/**
 * The actual game!!!!
 */
@loglevel
export default class GameInstance
{
  constructor()
  {
    this.log.info('Hello, World!');
  }
}
