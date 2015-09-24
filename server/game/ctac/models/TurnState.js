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
    // who
    this.currentPlayerId = null;

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
  passTurnTo(playerId)
  {
    this.currentPlayerId = playerId;
    this._turnStart = moment();
    this.currentTurn++;

    this.log.info('started turn %s, player %s', this.currentTurn, playerId);
  }
}
