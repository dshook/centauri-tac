import GamePiece from '../models/GamePiece.js';
import SpawnPiece from '../actions/SpawnPiece.js';
import Message from '../actions/Message.js';
import SetPlayerResource from '../actions/SetPlayerResource.js';
import ActivateCard from '../actions/ActivateCard.js';
import loglevel from 'loglevel-decorator';
import _ from 'lodash';

/**
 * Handle playing a card
 */
@loglevel
export default class ActivateCardProcessor
{
  constructor(playerResourceState, cardDirectory, pieceState, mapState)
  {
    this.playerResourceState = playerResourceState;
    this.cardDirectory = cardDirectory;
    this.pieceState = pieceState;
    this.mapState = mapState;
  }
  /**
   * Proc
   */
  async handleAction(action, queue)
  {
    if (!(action instanceof ActivateCard)) {
      return;
    }
    //check to see if they have enough energy to play
    let cardPlayed = this.cardDirectory.directory[action.cardId];
    if(cardPlayed.cost > this.playerResourceState.get(action.playerId)){
      this.log.info('Not enough resources for player %s to play card %s'
        , action.playerId, action.cardId);
      queue.cancel(action);
      queue.push(new Message('You don\'t have enough energy to play that card!'));
      return;
    }

    //check to make sure the card was played in a valid spot
    let playerHero = this.pieceState.hero(action.playerId);
    let kingDist = this.mapState.kingDistance(playerHero.position, action.position);
    if(kingDist > 1){
      this.log.info('Cannot play minion that far away, dist %s'
        , kingDist);
      queue.cancel(action);
      queue.push(new Message('You must play your minions close to your hero!'));
      return;
    }


    //all good if we make it this far
    queue.push(new SetPlayerResource(action.playerId, -cardPlayed.cost));
    queue.push(new SpawnPiece(action.playerId, action.cardId, action.position));

    queue.complete(action);
    this.log.info('player %s played card %s at %s',
      action.playerId, action.cardId, action.position);
  }
}
