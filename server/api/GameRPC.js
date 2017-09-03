import {rpc} from 'sock-harness';
import roles from '../middleware/rpc/roles.js';
import loglevel from 'loglevel-decorator';
import {on} from 'emitter-binder';

//TODO: Big problem: a lot of this code assumes there are only two players
// and broadcasts stuff to all clients that doesn't need to be broadcast to them
@loglevel
export default class GameRPC
{
  constructor(games, gameManager)
  {
    this.games = games;
    this.manager = gameManager;

    this.clients = new Set();
  }

  /**
   * Client wants the entire gamelist
   */
  // @rpc.command('gamelist')
  // @rpc.middleware(roles(['player']))
  // async sendGamelist(client)
  // {
  //   const games = await this.games.getActive();
  //   for (const g of games) {
  //     client.send('game', g);
  //   }
  // }

  /**
   * A game component is switching the allow join param
   */
  @on('update:allowJoin')
  async updateAllowJoin({gameId, allowJoin})
  {
    await this.manager.setAllowJoin(gameId, allowJoin);
  }

  /**
   * Game completed message
   */
  @on('game:completed')
  async completed({gameId, winningPlayerId})
  {
    await this.manager.completeGame(gameId, winningPlayerId);
  }

  /**
   * Client wants to create a game
   */
  @on('create')
  async createGame({name}, auth)
  {
    const playerId = auth.sub.id;
    await this.manager.create(name, playerId);
  }

  /**
   * Component is building a game for a set of players
   */
  @on('gamelist:createFor')
  async createGameFor({name, playerIds})
  {
    const [host, ...others] = playerIds;

    const game = await this.manager.create(name, host);

    if(!game) return;

    for (const id of others) {
      await this.manager.playerJoin(id, game.id);
    }
  }

  /**
   * A game component is informing us that a player has joined
   */
  @on('playerJoined')
  async playerJoined({gameId, playerId})
  {
    //Need to check if this is needed
    //this.manager.playerJoin(playerId, gameId);
  }

  /**
   * A game component is informing us that a player has parted
   */
  @on('playerParted')
  async playerParted({gameId, playerId})
  {
    this.manager.playerPart(playerId, gameId);
  }

  /**
   * Send game update to all connected
   */
  // @on('game')
  // _broadcastGame(game)
  // {
  //   for (const c of this.clients) {
  //     c.send('game', game);
  //   }
  // }

  /**
   * Send game remove to all connected
   */
  // @on('game:remove')
  // _broadcastRemoveGame(gameId)
  // {
  //   for (const c of this.clients) {
  //     c.send('game:remove', gameId);
  //   }
  // }


  /**
   * If a player is conencted, inform them of their current game
   */
  @on('game:current')
  _broadcastCurrentGame({game, playerId})
  {
    for (const c of this.clients) {
      const {id} = c.auth.sub;
      if (playerId === id) {
        c.send('game:current', game);
      }
    }
  }

  /**
   * When a client connects
   */
  @rpc.command('_token')
  async hello(client, params, auth)
  {
    // if no user, this is a component, and we dont need to track for
    // broadcasts?
    if (!auth.sub) {
      return;
    }

    this.clients.add(client);

    // const playerId = auth.sub.id;

    // // player already in a game?
    // const gId = await this.games.currentGameId(playerId);

    // if (!gId) {
    //   return;
    // }

    // const game = await this.games.getActive(gId);

    // // inform player of current game
    // client.send('game:current', game);
  }

  /**
   * Track connected clients
   */
  @rpc.disconnected()
  bye(client)
  {
    this.clients.delete(client);
  }

  /**
   * A player is requesting to join
   */
  @rpc.command('join')
  @rpc.middleware(roles(['player']))
  playerJoin(client, gameId, auth)
  {
    const playerId = auth.sub.id;
    this.manager.playerJoinGame(client, playerId, gameId);
  }

  /**
   * A player is trying to BOUNCE
   */
  @rpc.command('part')
  @rpc.middleware(roles(['player']))
  playerPart(client, params, auth)
  {
    const playerId = auth.sub.id;
    this.manager.playerPartGame(client, playerId);
  }

}
