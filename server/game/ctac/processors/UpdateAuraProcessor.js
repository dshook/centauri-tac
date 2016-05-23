import GamePiece from '../models/GamePiece.js';
import loglevel from 'loglevel-decorator';

/**
 * Update all auras based off the current piece state,
   this gets fired off at the end of the initial queue processing
 */
@loglevel
export default class UpdateAuraProcessor
{
  constructor(pieceState, cardEvaluator)
  {
    this.pieceState = pieceState;
    this.cardEvaluator = cardEvaluator;

    //keep track of which minions we've already 'activated'
    this.auraMinions = [];
  }

  /**
   * Proc
   */
  async handleAction(queue)
  {
    //find out newly added minions
      //add buffs to all selected pieces

    //find removed minions
      //remove buffs from all selected pieces

    //find remaining minions that still might have moved/changed
      //find all affected pieces, compare with affected pieces last process
      //add or remove buffs as necessary


    this.log.info('Updated auras');
  }
}
