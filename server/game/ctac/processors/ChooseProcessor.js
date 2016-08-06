import Choose from '../actions/Choose.js';
import loglevel from 'loglevel-decorator';

/**
 * Handle eval'ing the choice from a card
 */
@loglevel
export default class ChooseProcessor
{
  constructor(cardDirectory, cardEvaluator)
  {
    this.cardDirectory = cardDirectory;
    this.cardEvaluator = cardEvaluator;
  }
  /**
   * Proc
   */
  async handleAction(action, queue)
  {
    if (!(action instanceof Choose)) {
      return;
    }

    //make sure it's a legit choice
    if(action.selectedChoice !== action.choice1 && action.selectedChoice !== action.choice2){
      this.log.error('Invalid choice of %s from [%s, %s]', action.selectedChoice, action.choice1, action.choice2);
      return queue.cancel(action);
    }

    //find card choice in directory
    let cardPlayed = this.cardDirectory.directory[action.selectedChoice];
    if(!cardPlayed){
      this.log.error('Cannot find card %s for choice', action.selectedChoice);
      return queue.cancel(action);
    }
    if(!cardPlayed.isSpell){
      this.log.error('Choice cards must be spells :) %s', cardPlayed.name);
      return queue.cancel(action);
    }
    this.log.info('chose card %s', cardPlayed.name);

    if(this.cardEvaluator.evaluateSpellEvent('playSpell', {
      spellCard: cardPlayed,
      playerId: action.playerId,
      targetPieceId: action.targetPieceId,
      position: action.position,
      pivotPosition: action.pivotPosition,
      activatingPiece: action.activatingPiece,
      selfPiece: action.selfPiece,
    })){

      queue.complete(action);
      this.log.info('chose %s successful', cardPlayed.name);
    }else{
      //choose fizzles but no other consequences right now
      queue.cancel(action);
      this.log.info('Choice %s SCRUBBED', cardPlayed.name);
    }

  }
}
