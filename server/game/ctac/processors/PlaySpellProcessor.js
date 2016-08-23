import PlaySpell from '../actions/PlaySpell.js';
import loglevel from 'loglevel-decorator';
import SetPlayerResource from '../actions/SetPlayerResource.js';

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

    const playedCard = this.cardState.validateInHand(action.playerId, action.cardInstanceId);
    if(!playedCard){
      this.log.error('Card id %s was not found in player %s\'s hand', action.cardInstanceId, action.playerId);
      return queue.cancel(action, true);
    }

    if(this.cardEvaluator.evaluateSpellEvent('playSpell', {
      spellCard: playedCard,
      playerId: action.playerId,
      targetPieceId: action.targetPieceId,
      position: action.position,
      pivotPosition: action.pivotPosition,
      chooseCardTemplateId: action.chooseCardTemplateId
    })){

      this.cardState.playCard(action.playerId, action.cardInstanceId);
      queue.complete(action);
      queue.push(new SetPlayerResource(action.playerId, -playedCard.cost));
      this.statsState.stats['COMBOCOUNT']++;
      this.log.info('played spell %s for player %s at %s target %s',
        playedCard.name, action.playerId, action.position, action.targetPieceId);
    }else{
      //be sure to emit the cancel event so the client can respond
      queue.cancel(action, true);
      this.log.info('Play spell %s for player %s SCRUBBED',
        playedCard.name, action.playerId);
    }
  }
}
