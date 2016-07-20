import SetPlayerResource from '../actions/SetPlayerResource.js';
import Message from '../actions/Message.js';
import ActivateAbility from '../actions/ActivateAbility.js';
import loglevel from 'loglevel-decorator';

/**
 * Handle a piece using an ability
 */
@loglevel
export default class ActivateAbilityProcessor
{
  constructor(playerResourceState, pieceState, cardEvaluator)
  {
    this.playerResourceState = playerResourceState;
    this.pieceState = pieceState;
    this.cardEvaluator = cardEvaluator;
  }
  /**
   * Proc
   */
  async handleAction(action, queue)
  {
    if (!(action instanceof ActivateAbility)) {
      return;
    }

    //find piece on board
    var piece = this.pieceState.piece(action.pieceId);
    if(!piece){
      this.log.warn('Cannot use ability of piece %s when it is not in play', action.pieceId);
      queue.push(new Message('Piece Must be on the board to use an ability!'));
      return queue.cancel(action);
    }

    let ability = piece.events.find(e => e.event === 'ability');
    if(!ability){
      this.log.warn('Piece %s does not have an ability to use', piece.name);
      return queue.cancel(action);
    }

    let abilityCost = ability.args[0];
    let abilityChargeTime = ability.args[1];
    let abilityName = ability.args[2];
    //check to see if they have enough energy to play
    if(abilityCost > this.playerResourceState.get(piece.playerId)){
      this.log.warn('Not enough resources for player %s to use ability %j'
        , piece.playerId, ability);
      queue.push(new Message('You don\'t have enough energy to activate the ability!'));
      return queue.cancel(action);
    }

    if(piece.abilityCharge < abilityChargeTime){
      this.log.warn('Ability not charged. Pice abilityCharge %s need %s'
        , piece.abilityCharge, abilityChargeTime);
      queue.push(new Message('Ability needs time to charge!'));
      return queue.cancel(action);
    }

    if(this.cardEvaluator.evaluatePieceEvent('ability', piece, action.targetPieceId)){
      queue.complete(action);

      //use up all the ability charge on the piece
      piece.abilityCharge = 0;

      this.log.info('used ability %s for player %s',
        ability.args[2], piece.playerId);
      queue.push(new SetPlayerResource(piece.playerId, -abilityCost));
    }else{
      //be sure to emit the cancel event so the client can respond
      this.log.info('Ability %s for player %s SCRUBBED',
        abilityName, piece.playerId);
      return queue.cancel(action, true);
    }
  }
}
