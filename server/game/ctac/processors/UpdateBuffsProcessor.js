import PieceBuff from '../actions/PieceBuff.js';
import loglevel from 'loglevel-decorator';
import attributes from '../util/Attributes.js';

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
        let buffChange = null;

        //check to see if the buff has any eNumber attributes and if they've changed
        // this.log.info('Updating buff attributes %j', buff);
        for(let buffAttribute of buff.buffAttributes){
          if(!buffAttribute.attribute || (!buffAttribute.amount.eNumber && !buffAttribute.amount.eValue)) continue;

          let pieceSelectorParams = {
            selfPiece: piece,
            controllingPlayerId: piece.playerId,
            isSpell: false
          };
          let attr = buffAttribute.attribute;

          let newAmount = this.selector.eventualNumber(buffAttribute.amount, pieceSelectorParams);
          if(newAmount != buff[attr]){
            buffChange = buffChange || {};
            buffChange[attr] = newAmount - buff[attr];
            // this.log.info('buff change attr %s newAmt %s buffAmt %s', attr, newAmount, buff[attr]);
          }
        }
        if(buffChange){
          buffChange = piece.changeBuffStats(buffChange, {}, true);
          // this.log.info('buff change after apply %j', buffChange);

          //save total buff changes in the buff so if they get updated again they'll be accurate to find a new diff off of
          //also change the buff change to show the same numbers
          for(let attrib of attributes){
            if(buffChange[attrib] === undefined) continue;

            let newAttrib = 'new' + attrib.charAt(0).toUpperCase() + attrib.slice(1);
            buff[attrib] = buff[attrib] + buffChange[attrib];
            buff[newAttrib] = buffChange[newAttrib];
            buffChange[attrib] = buff[attrib];
          }
          buffChange.statuses = piece.statuses;

          // this.log.info('buff apply %j', buff);
        }

        //Update any conditional buffs enabling or disabling them if neccessary
        if(buff.condition){
          let result = this.selector.compareExpression(
            buff.condition,
            this.pieceState.pieces,
            {
              selfPiece: piece,
              controllingPlayerId: piece.playerId
            },
            this.selector.selectPieces
          );

          if(result.length > 0){
            if(!buff.enabled){
              //switch to enabled
              buffChange = piece.enableBuff(buff, this.cardEvaluator);
            }
          }else{
            if(buff.enabled){
              //switch to disabled
              buffChange = piece.disableBuff(buff, this.cardEvaluator);
            }
          }

        }

        if(buffChange){
          let clientAction = new PieceBuff({buffId: buff.buffId, pieceId: piece.id, name: buff.name});
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
