import GamePiece from '../models/GamePiece.js';
import SpawnPiece from '../actions/SpawnPiece.js';
import loglevel from 'loglevel-decorator';
import Random from '../util/Random.js';
import _ from 'lodash';

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
    let nextId = this.pieceState.pieces.length == 0 ? 1 :
       _.max(this.pieceState.pieces, x => x.id).id + 1;

    //TODO: validate against board state and all that jazz
    var newPiece = new GamePiece();
    newPiece.id = nextId;
    newPiece.position = action.position;
    newPiece.playerId = action.playerId;
    newPiece.resourceId = action.pieceResourceId;
    newPiece.attack = Random.Range(1, 10);
    newPiece.health = Random.Range(1, 10);

    action.attack = newPiece.attack;
    action.health = newPiece.health;
    action.pieceId = newPiece.id;

    this.pieceState.pieces.push(newPiece);

    queue.complete(action);
    this.log.info('spawned piece %s for player %s at %s',
      action.pieceResourceId, action.playerId, action.position);
  }
}
