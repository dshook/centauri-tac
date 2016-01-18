import GamePiece from '../models/GamePiece.js';
import SpawnPiece from '../actions/SpawnPiece.js';
import loglevel from 'loglevel-decorator';

/**
 * Handle the PassTurn action
 */
@loglevel
export default class SpawnProcessor
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

    let cardPlayed = this.cardDirectory.directory[action.cardId];

    var newPiece = new GamePiece();
    newPiece.position = action.position;
    newPiece.playerId = action.playerId;
    newPiece.name = cardPlayed.name;
    newPiece.cardId = action.cardId;
    newPiece.attack = cardPlayed.attack;
    newPiece.health = cardPlayed.health;
    newPiece.baseAttack = cardPlayed.attack;
    newPiece.baseHealth = cardPlayed.health;
    newPiece.movement = cardPlayed.movement;
    newPiece.baseMovement = cardPlayed.movement;
    newPiece.tags = cardPlayed.tags;

    action.pieceId = this.pieceState.add(newPiece);
    action.tags = newPiece.tags;

    this.cardEvaluator.evaluatePieceEvent('playMinion', newPiece);

    queue.complete(action);
    this.log.info('spawned piece %s for player %s at %s',
      action.cardId, action.playerId, action.position);
  }
}
