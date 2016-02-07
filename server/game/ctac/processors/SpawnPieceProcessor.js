import GamePiece from '../models/GamePiece.js';
import SpawnPiece from '../actions/SpawnPiece.js';
import SetPlayerResource from '../actions/SetPlayerResource.js';
import loglevel from 'loglevel-decorator';

/**
 * Handle the PassTurn action
 */
@loglevel
export default class SpawnPieceProcessor
{
  constructor(pieceState, players, cardDirectory, cardEvaluator)
  {
    this.pieceState = pieceState;
    this.players = players;
    this.cardDirectory = cardDirectory;
    this.cardEvaluator = cardEvaluator;
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

    var newPiece = new GamePiece();
    newPiece.position = action.position;
    newPiece.playerId = action.playerId;
    newPiece.name = cardPlayed.name;
    newPiece.cardTemplateId = action.cardTemplateId;
    newPiece.attack = cardPlayed.attack;
    newPiece.health = cardPlayed.health;
    newPiece.baseAttack = cardPlayed.attack;
    newPiece.baseHealth = cardPlayed.health;
    newPiece.movement = cardPlayed.movement;
    newPiece.baseMovement = cardPlayed.movement;
    newPiece.tags = cardPlayed.tags;

    if(this.cardEvaluator.evaluatePieceEvent('playMinion', newPiece, action.targetPieceId)){
      action.pieceId = this.pieceState.add(newPiece);
      action.tags = newPiece.tags;

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
