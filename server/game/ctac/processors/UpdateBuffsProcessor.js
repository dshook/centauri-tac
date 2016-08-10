import PieceBuff from '../actions/PieceBuff.js';
import loglevel from 'loglevel-decorator';

/**
 * Update all buffs with a condition
   this gets fired off at the end of the initial queue processing
 */
@loglevel
export default class UpdateBuffsProcessor
{
  constructor(pieceState, cardEvaluator, selector)
  {
    this.pieceState = pieceState;
    this.cardEvaluator = cardEvaluator;
    this.selector = selector;
  }

  /**
   * Proc
   */
  async handleAction(action, queue)
  {
    for(let piece of this.pieceState.pieces){
      if(piece.buffs.length === 0) continue;

      for(let buff of piece.buffs){
        if(!buff.condition) continue;

        let result = this.selector.compareExpression(buff.condition, this.pieceState.pieces,
          {
            selfPiece: piece,
            controllingPlayerId: piece.playerId
          });

        let buffChange = null;
        if(result.length > 0){
          if(!buff.enabled){
            //switch to enabled
            buffChange = piece.enableBuff(buff);
          }
        }else{
          if(buff.enabled){
            //switch to disabled
            buffChange = piece.disableBuff(buff);
          }
        }

        if(buffChange){
          let clientAction = new PieceBuff(piece.id, buff.name, !buff.enabed);
          for(let buffKey in buffChange){
            clientAction[buffKey] = buffChange[buffKey];
          }
          this.autoCompleteAction(clientAction, queue);
        }
      }
    }

    this.log.info('Updated buffs');
  }

  //the buffs generated here shouldn't go through the normal piece buff processor,
  //but need to be sent to the client, so we add them to the front of the queue and then complete
  autoCompleteAction(action, queue){
    action.alreadyComplete = true;
    queue.pushFront(action);
  }
}
