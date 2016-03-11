import GamePiece from '../models/GamePiece.js';
import SpawnPiece from '../actions/SpawnPiece.js';
import SetPlayerResource from '../actions/SetPlayerResource.js';
import Message from '../actions/Message.js';
import loglevel from 'loglevel-decorator';

/**
 * Handle the PassTurn action
 */
@loglevel
export default class SpawnPieceProcessor
{
  constructor(pieceState, players, cardDirectory, cardEvaluator, cardState)
  {
    this.pieceState = pieceState;
    this.players = players;
    this.cardDirectory = cardDirectory;
    this.cardEvaluator = cardEvaluator;
    this.cardState = cardState;
  }

  /**
   * Proc
   */
  async handleAction(action, queue)
  {
    if (!(action instanceof SpawnPiece)) {
      return;
    }

    let cardPlayed = this.cardDirectory.directory[action.cardTemplateId];

    //check if played in an unoccupied spot
    let occupyingPiece = this.pieceState.pieceAt(action.position.x, action.position.z);
    if(occupyingPiece){
      //be sure to emit the cancel event so the client can respond
      queue.cancel(action, true);
      this.log.warn('Can\'t spawn piece %s at position %s because %j is occupying it',
        cardPlayed.name, action.position, occupyingPiece);
      return;
    }

    var newPiece = this.pieceState.newFromCard(this.cardDirectory, action.cardTemplateId, action.playerId, action.position);

    if(this.cardEvaluator.evaluatePieceEvent('playMinion', newPiece, action.targetPieceId)){
      action.pieceId = this.pieceState.add(newPiece);
      action.tags = newPiece.tags;

      if(action.cardInstanceId !== null){
        const playedCard = this.cardState.playCard(action.playerId, action.cardInstanceId);
        if(!playedCard){
          this.log.error('Card id %s was not found in player %s\'s hand', action.cardInstanceId, action.playerId);
          queue.cancel(action, true);
          return;
        }
      }

      queue.complete(action);
      this.log.info('spawned piece %s for player %s at %s',
        action.cardTemplateId, action.playerId, action.position);
      queue.push(new SetPlayerResource(action.playerId, -cardPlayed.cost));
    }else{
      //be sure to emit the cancel event so the client can respond
      queue.cancel(action, true);
      this.log.info('Spawned piece %s for player %s SCRUBBED',
        action.cardTemplateId, action.playerId, action.position);
    }

  }
}
