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
    if (!(action instanceof PlaySpell)) {
      return;
    }

    let cardPlayed = this.cardDirectory.directory[action.cardTemplateId];


    if(this.cardEvaluator.evaluateSpellEvent('playSpell', cardPlayed, action.playerId, action.targetPieceId)){
      if(action.cardInstanceId !== null){
        const playedCard = this.cardState.playCard(action.playerId, action.cardInstanceId);
        if(!playedCard){
          this.log.error('Card id %s was not found in player %s\'s hand', action.cardInstanceId, action.playerId);
          queue.cancel(action, true);
        }
      }

      queue.complete(action);
      queue.push(new SetPlayerResource(action.playerId, -cardPlayed.cost));
      this.log.info('played spell %s for player %s at %s target %s',
        cardPlayed.name, action.playerId, action.position, action.targetPieceId);
    }else{
      //be sure to emit the cancel event so the client can respond
      queue.cancel(action, true);
      this.log.info('Play spell %s for player %s SCRUBBED',
        cardPlayed.name, action.playerId);
    }
  }
}
