import GamePiece from '../models/GamePiece.js';
import SpawnPiece from '../actions/SpawnPiece.js';
import loglevel from 'loglevel-decorator';

/**
 * Handle the PassTurn action
 */
@loglevel
export default class SpawnProcessor
{
  constructor(pieceState, players)
  {
    this.pieceState = pieceState;
    this.players = players;
  }

  /**
   * Proc
   */
  async handleAction(action, queue)
  {
    if (!(action instanceof SpawnPiece)) {
      return;
    }

    //TODO: validate against board state and all that jazz
    var newPiece = new GamePiece();
    newPiece.position = action.position;
    newPiece.playerId = action.playerId;
    newPiece.resourceId = action.pieceResourceId;

    this.pieceState.pieces.push(newPiece);

    queue.complete(action);
    this.log.info('spawned piece %s for player %s at %s',
      action.pieceResourceId, action.playerId, action.position);
  }
}
