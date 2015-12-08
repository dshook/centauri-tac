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
    let nextId = this.pieceState.pieces.length == 0 ? 1 :
       _.max(this.pieceState.pieces, x => x.id).id + 1;

    let cardPlayed = this.cardDirectory.directory[action.cardId];

    //TODO: validate against board state and all that jazz
    var newPiece = new GamePiece();
    newPiece.id = nextId;
    newPiece.position = action.position;
    newPiece.playerId = action.playerId;
    newPiece.cardId = action.cardId;
    newPiece.attack = cardPlayed.attack;
    newPiece.health = cardPlayed.health;
    newPiece.baseAttack = cardPlayed.attack;
    newPiece.baseHealth = cardPlayed.health;
    newPiece.movement = cardPlayed.movement;
    newPiece.baseMovement = cardPlayed.movement;
    newPiece.tags = cardPlayed.tags;

    action.pieceId = newPiece.id;

    this.pieceState.pieces.push(newPiece);

    this.cardEvaluator.evaluateAction('play', newPiece);

    queue.complete(action);
    this.log.info('spawned piece %s for player %s at %s',
      action.cardId, action.playerId, action.position);
  }
}
