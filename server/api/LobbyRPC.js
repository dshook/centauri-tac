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
    this.clients = {};
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
    await this.matchmaker.queuePlayer(playerId, deckId);
  }

  /**
   * And back out again
   */
  @rpc.command('dequeue')
  @rpc.middleware(roles(['player']))
  async dequeuePlayer(client, params, auth)
  {
    const playerId = auth.sub.id;
    await this.matchmaker.dequeuePlayer(playerId);
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

    let playerId = client && client.auth ? client.auth.sub.id : null;

    if(!playerId){
      this.log.warn('Connecting client missing auth creds');
      return;
    }

    if(this.clients[playerId]){
      this.disconnectClient(this.clients[playerId], 'reconnect')
    }

    this.clients[playerId] = client;

    // drop player when they DC
    client.once('close', () => this.disconnectClient(client, 'close'));
    client.once('error', () => this.disconnectClient(client, 'error'));
    client.once('disconnected', () => this.disconnectClient(client, 'disconnect'));
  }

  disconnectClient(client, reason){
    let playerId = client && client.auth ? client.auth.sub.id : null;
    this.log.info('Closing connection for %s for player %s', reason, playerId);
    if(playerId){
      this.matchmaker.dequeuePlayer(playerId);
      delete this.clients[playerId];
    }
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

  sendToPlayer(playerId, message, data){
    // this.log.info('Lobby Clients %j', Object.keys(this.clients));
    let client = this.clients[playerId];
    if(client){
      client.send(message, data);
    }
  }
}
