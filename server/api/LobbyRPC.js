import {rpc} from 'sock-harness';
import loglevel from 'loglevel-decorator';
import roles from '../middleware/rpc/roles.js';
import {on} from 'emitter-binder';
import PlayerDeck from 'models/PlayerDeck';
import DeckStoreError from 'errors/DeckStoreError';

/**
 * RPC handler for the matchmaker component
 */
@loglevel
export default class LobbyRPC
{
  constructor(matchmaker, cardManager)
  {
    this.matchmaker = matchmaker;
    this.cardManager = cardManager;
    this.clients = new Set();
  }

  /**
   * Drop a player into the queue
   */
  @rpc.command('queue')
  @rpc.middleware(roles(['player']))
  async queuePlayer(client, params, auth)
  {
    const playerId = auth.sub.id;
    const {deckId} = params;
    await this.matchmaker.queuePlayer(client, playerId, deckId);
  }

  /**
   * And back out again
   */
  @rpc.command('dequeue')
  @rpc.middleware(roles(['player']))
  async dequeuePlayer(client, params, auth)
  {
    const playerId = auth.sub.id;
    await this.matchmaker.dequeuePlayer(client, playerId);
  }

  /**
   * When a client connects
   */
  @rpc.command('_token')
  async hello(client, params, auth)
  {
    // connection from in the mesh
    if (!auth || !auth.sub) {
      return;
    }

    this.clients.add(client);

    // drop player when they DC
    const playerId = auth.sub.id;
    client.once('close', () => this.matchmaker.dequeuePlayer(playerId, client));
    client.once('close', () => this.clients.delete(client));
  }

  /**
   * If a player is connected, inform them of their current game
   * don't think this is needed anymore,
   */
  @on('game:current')
  _broadcastCurrentGame({game, playerId})
  {
    this.sendToPlayer(playerId, 'game:current', game);
  }

  /**
   * broadcast status
   */
  @on('matchmaker:status')
  _status(status)
  {
    this.sendToPlayer(status.playerId, 'status', status);
  }

  /**
   * Get decks, duh
   */
  @rpc.command('getDecks')
  @rpc.middleware(roles(['player']))
  async getDecks(client, params, auth)
  {
    const playerId = auth.sub.id;
    let decks = await this.cardManager.getDecks(playerId);
    this.sendToPlayer(playerId, 'decks:current', decks);
  }

  @rpc.command('saveDeck')
  @rpc.middleware(roles(['player']))
  async saveDeck(client, params, auth)
  {
    const playerId = auth.sub.id;
    let deck;
    try{
      deck = PlayerDeck.fromData(params);
      deck = await this.cardManager.saveDeck(playerId, deck);
    }catch(e){
      this.log.warn('Deck Save failed for player %s, reason: %s', playerId, e.message);
      let message = e.message;
      if (!(e instanceof DeckStoreError)) {
        message = 'Error saving deck, please try again later';
      }
      this.sendToPlayer(playerId, 'decks:saveFailed', message);
      return;
    }

    this.log.info('Created/Saved deck %s for player %s', deck.id, playerId);
    this.sendToPlayer(playerId, 'decks:saveSuccess', deck);
  }

  @rpc.command('deleteDeck')
  @rpc.middleware(roles(['player']))
  async deleteDeck(client, deckId, auth)
  {
    const playerId = auth.sub.id;
    try{
      await this.cardManager.deleteDeck(playerId, deckId);
    }catch(e){
      this.log.warn('Deck delete failed for player %s, reason: %s', playerId, e.message);
      let message = e.message;
      if (!(e instanceof DeckStoreError)) {
        message = 'Error deleting deck, please try again later';
      }
      this.sendToPlayer(playerId, 'decks:saveFailed', message);
      return;
    }
    this.log.info('Deleted deck %s for player %s', deckId, playerId);
  }

  //prolly should index them
  sendToPlayer(playerId, message, data){
    for (const c of this.clients) {
      const {id} = c.auth.sub;
      if (playerId === id) {
        c.send(message, data);
      }
    }
  }
}
