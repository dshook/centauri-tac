import GamePiece from '../models/GamePiece.js';
import SpawnPiece from '../actions/SpawnPiece.js';
import PlaySpell from '../actions/PlaySpell.js';
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
    this.log.info('found card %j', cardPlayed);
    if(!cardPlayed){
      this.log.info('Cannot find card %s in player %s\'s hand'
        , action.cardInstanceId, action.player);
      queue.cancel(action);
      return;
    }

    //check to see if they have enough energy to play
    if(cardPlayed.cost > this.playerResourceState.get(action.playerId)){
      this.log.info('Not enough resources for player %s to play card %s'
        , action.playerId, cardPlayed.cardId);
      queue.cancel(action);
      queue.push(new Message('You don\'t have enough energy to play that card!'));
      return;
    }

    //check to make sure the card was played in a valid spot
    if(cardPlayed.hasTag('Minion')){
      let playerHero = this.pieceState.hero(action.playerId);
      let kingDist = this.mapState.kingDistance(playerHero.position, action.position);
      if(kingDist > 1){
        this.log.info('Cannot play minion that far away, dist %s'
          , kingDist);
        queue.cancel(action);
        queue.push(new Message('You must play your minions close to your hero!'));
        return;
      }
    }

    //all good if we make it this far
    let cardWasInHand = this.cardState.playCard(action.playerId, action.cardInstanceId);
    if(!cardWasInHand){
        this.log.info('Card id %s was not found in player %s\'s hand', action.cardInstanceId, action.playerId);
        queue.cancel(action);
        queue.push(new Message('Cards must be in your hand to play them!'));
        return;
      }
    queue.push(new SetPlayerResource(action.playerId, -cardPlayed.cost));

    if(cardPlayed.hasTag('Minion')){
      queue.push(new SpawnPiece(action.playerId, cardPlayed.cardId, action.position));
    }else if(cardPlayed.hasTag('Spell')){
      queue.push(new PlaySpell(action.playerId, cardPlayed.cardId, action.position));
    }else{
      throw 'Card played must be either a minion or a spell';
    }

    queue.complete(action);
    this.log.info('player %s played card %s at %s',
      action.playerId, cardPlayed.name, action.position);
  }
}
