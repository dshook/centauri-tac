import GamePiece from '../models/GamePiece.js';
import loglevel from 'loglevel-decorator';

/**
 * Update all auras based off the current piece state,
   this gets fired off at the end of the initial queue processing
 */
@loglevel
export default class UpdateAuraProcessor
{
  constructor(pieceState, players)
  {
    this.pieceState = pieceState;
    this.players = players;
  }

  /**
   * Proc
   */
  async handleAction(queue)
  {
    this.log.info('Gonna update auras someday %s', this.pieceState.pieces.length);
  }
}
