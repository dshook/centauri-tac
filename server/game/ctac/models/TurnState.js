import moment from 'moment';
import loglevel from 'loglevel-decorator';

/**
 * Current state of the turn system
 */
@loglevel
export default class TurnState
{
  constructor()
  {
    // which
    this.currentTurn = 0;

    // how long
    this._turnStart = moment();
  }

  /**
   * How long
   */
  get currentTimeInSeconds()
  {
    return moment().diff(this._turnStart, 'seconds');
  }

  /**
   * Next turn
   */
  passTurn()
  {
    this._turnStart = moment();
    this.currentTurn++;

    this.log.info('started turn %s', this.currentTurn);
    return this.currentTurn;
  }
}
