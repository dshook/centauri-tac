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
  constructor(playerResourceState, pieceState, cardEvaluator, selector)
  {
    this.playerResourceState = playerResourceState;
    this.pieceState = pieceState;
    this.cardEvaluator = cardEvaluator;
    this.selector = selector;
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
    let playerId = piece.playerId;
    //Check to see if the piece ability is for the other player
    if(ability.args[3]){
      playerId = this.selector.selectPlayer(piece.playerId, ability.args[3]);
    }

    //check to see if they have enough energy to play
    if(abilityCost > this.playerResourceState.get(playerId)){
      this.log.warn('Not enough resources for player %s to use ability %j'
        , playerId, ability);
      queue.push(new Message('You don\'t have enough energy to activate the ability!', playerId));
      return queue.cancel(action);
    }

    if(piece.abilityCharge < abilityChargeTime){
      this.log.warn('Ability not charged. Pice abilityCharge %s need %s'
        , piece.abilityCharge, abilityChargeTime);
      queue.push(new Message('Ability needs time to charge!', playerId));
      return queue.cancel(action);
    }

    if(this.cardEvaluator.evaluatePieceEvent('ability', piece, {targetPieceId: action.targetPieceId})){
      queue.complete(action);

      //use up all the ability charge on the piece
      piece.abilityCharge = 0;

      this.log.info('used ability %s for player %s',
        ability.args[2], playerId);
      queue.push(new SetPlayerResource(playerId, -abilityCost));
    }else{
      //be sure to emit the cancel event so the client can respond
      this.log.info('Ability %s for player %s SCRUBBED',
        abilityName, playerId);
      return queue.cancel(action, true);
    }
  }
}
