import GamePiece from '../models/GamePiece.js';
import CharmPiece from '../actions/CharmPiece.js';
import loglevel from 'loglevel-decorator';

/**
 * Handle the PassTurn action
 */
@loglevel
export default class CharmPieceProcessor
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
    if (!(action instanceof CharmPiece)) {
      return;
    }

    var piece = this.pieceState.piece(action.pieceId);
    if(!piece){
      this.log.warn('Could not find piece %s to charm %j', action.pieceId, this.pieceState);
      return queue.cancel(action);
    }

    let newPlayer = this.players.find(p => p.id != piece.playerId)

    if(!newPlayer){
      this.log.warn('Could not find new player for charm %j, player %s', this.players, piece.playerId);
      return queue.cancel(action);
    }

    piece.playerId = newPlayer.id;
    action.newPlayerId = piece.playerId;

    queue.complete(action);
    this.log.info('Charmed piece %s new owner %s',
      piece.id, piece.playerId);
  }
}
