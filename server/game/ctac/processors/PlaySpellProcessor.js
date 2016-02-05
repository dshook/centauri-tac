import GamePiece from '../models/GamePiece.js';
import PlaySpell from '../actions/PlaySpell.js';
import loglevel from 'loglevel-decorator';
import SetPlayerResource from '../actions/SetPlayerResource.js';

/**
 * Handle the PassTurn action
 */
@loglevel
export default class PlaySpellProcessor
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
    if (!(action instanceof PlaySpell)) {
      return;
    }

    let cardPlayed = this.cardDirectory.directory[action.cardTemplateId];

    this.cardEvaluator.evaluateSpellEvent('playSpell', cardPlayed, action.playerId, action.targetPieceId);

    queue.complete(action);
    queue.push(new SetPlayerResource(action.playerId, -cardPlayed.cost));
    this.log.info('played spell %s for player %s at %s',
      cardPlayed.name, action.playerId, action.position);
  }
}
