import SpawnPiece from '../actions/SpawnPiece.js';
import PlaySpell from '../actions/PlaySpell.js';
import Message from '../actions/Message.js';
import ActivateCard from '../actions/ActivateCard.js';
import Statuses from '../models/Statuses.js';
import loglevel from 'loglevel-decorator';

/**
 * Handle playing a card
 */
@loglevel
export default class ActivateCardProcessor
{
  constructor(playerResourceState, cardDirectory, pieceState, mapState, cardState)
  {
    this.playerResourceState = playerResourceState;
    this.cardDirectory = cardDirectory;
    this.pieceState = pieceState;
    this.mapState = mapState;
    this.cardState = cardState;
  }
  /**
   * Proc
   */
  async handleAction(action, queue)
  {
    if (!(action instanceof ActivateCard)) {
      return;
    }

    //find card in hand
    let cardPlayed = this.cardState.hands[action.playerId].find(c => c.id === action.cardInstanceId);
    if(!cardPlayed){
      this.log.warn('Cannot find card %s in player %s\'s hand', action.cardInstanceId, action.playerId);
      queue.push(new Message('Cards must be in your hand to play them!', action.playerId));
      return queue.cancel(action, true);
    }
    this.log.info('found card %s', cardPlayed.name);

    //check to see if they have enough energy to play
    if(cardPlayed.cost > this.playerResourceState.get(action.playerId)){
      this.log.warn('Not enough resources for player %s to play card %s'
        , action.playerId, cardPlayed.id);
      queue.push(new Message('You don\'t have enough energy to play that card!', action.playerId));
      return queue.cancel(action, true);
    }

    //check to make sure the card was played in a valid spot
    if(cardPlayed.isMinion){
      let playerHero = this.pieceState.hero(action.playerId);
      let allowableDistance = (cardPlayed.statuses & Statuses.Airdrop) != 0 ? 4 : 1;
      let kingDist = this.mapState.kingDistance(playerHero.position, action.position);
      if(kingDist > allowableDistance){
        this.log.warn('Cannot play minion that far away, dist %s'
          , kingDist);
        queue.push(new Message('You must play your minions close to your hero!', action.playerId));
        return queue.cancel(action, true);
      }

      //check height differential
      let currentTile = this.mapState.getTile(playerHero.position);
      let destinationTile = this.mapState.getTile(action.position)
      if(!this.mapState.isHeightPassable(currentTile, destinationTile)){
        this.log.warn('Cannot play minion up that much height diff');
        queue.push(new Message("Can't reach!", action.playerId));
        return queue.cancel(action, true);
      }
      if(destinationTile.unpassable){
        this.log.warn('Cannot play minion on unpassable tile');
        queue.push(new Message("Can't play minion on that tile!", action.playerId));
        return queue.cancel(action, true);
      }

      //mostly all good if we make it this far, individual processors could still potentiall cancel their own action
      queue.push(new SpawnPiece(
        {
          playerId: action.playerId,
          cardTemplateId: cardPlayed.cardTemplateId,
          position: action.position,
          cardInstanceId: action.cardInstanceId,
          targetPieceId: action.targetPieceId,
          pivotPosition: action.pivotPosition,
          chooseCardTemplateId: action.chooseCardTemplateId
        }
      ));

    }else if(cardPlayed.isSpell){
      queue.push(new PlaySpell(
        action.playerId,
        action.cardInstanceId,
        cardPlayed.cardTemplateId,
        action.position,
        action.targetPieceId,
        action.pivotPosition,
        action.chooseCardTemplateId
      ));
    }else{
      throw 'Card played must be either a minion or a spell';
    }

    queue.complete(action);
    this.log.info('player %s played card %s at %s',
      action.playerId, cardPlayed.name, action.position);
  }
}
