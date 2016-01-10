import GamePiece from '../models/GamePiece.js';
import PlaySpell from '../actions/PlaySpell.js';
import loglevel from 'loglevel-decorator';

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

    let cardPlayed = this.cardDirectory.directory[action.cardId];

    this.cardEvaluator.evaluateSpellEvent('playSpell', cardPlayed, action.playerId);

    queue.complete(action);
    this.log.info('played spell %s for player %s at %s',
      cardPlayed.name, action.playerId, action.position);
  }
}
