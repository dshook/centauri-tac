import loglevel from 'loglevel-decorator';

/**
 * Game stats state, some of which is accessible to cards
 */
@loglevel
export default class StatsState
{
  constructor()
  {
    this.stats = {
      COMBOCOUNT: 0
    };
  }
}
