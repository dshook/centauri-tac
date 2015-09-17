import loglevel from 'loglevel-decorator';
import Game from 'models/Game';
import Component from 'models/Component';
import ComponentType from 'models/ComponentType';
import Player from 'models/Player';
import GameState from 'models/GameState';
import {EventEmitter} from 'events';
import hrtime from 'hrtime-log-decorator';

/**
 * Data layer for games
 */
@loglevel
export default class GameStore extends EventEmitter
{
  constructor(sql, messenger)
  {
    super();
    this.sql = sql;
    this.messenger = messenger;
  }

  /**
   * Active games on realm (or a single one)
   */
  @hrtime('queried games in %s ms')
  async getActive(realm = null, id = null)
  {
    let sql = `

      select g.*, c.*, t.*, p.*, s.*, counts.current_player_count
      from games as g
      join components c
        on g.game_component_id = c.id
      join component_types t
        on c.component_type_id = t.id
      join players p
        on g.host_player_id = p.id
      left join game_states s
        on g.game_state_id = s.id

      -- show curent player count
      left join (select game_id, count(*) as current_player_count
          from game_players
          group by game_id) as counts
        on g.id = counts.game_id

      where c.is_active

    `;

    const params = {};

    if (realm !== null) {
      sql += ' and c.realm = @realm ';
      params.realm = realm;
    }

    if (id !== null) {
      sql += ' and g.id = @id ';
      params.id = id;
    }

    const models = [Game, Component, ComponentType, Player, GameState];

    const resp = await this.sql.tquery(...models)(sql, params,
      (g, c, t, p, gs, cpc) => {
        g.component = c;
        g.component.type = t;
        g.hostPlayer = p;
        g.state = gs;
        g.currentPlayerCount = 0 | cpc['current_player_count'];
        return g;
      });

    // single
    if (id) {
      return resp.firstOrNull();
    }

    return resp.toArray();
  }

  /**
   * Join player to a game, need to make sure they're disconnected elswere
   */
  async playerJoin(playerId, gameId)
  {
    // make sure the player leaves current game first
    await this.playerPart(playerId);

    await this.sql.query(`
        insert into game_players (player_id, game_id)
        values (@playerId, @gameId)
        `, {playerId, gameId});

    const game = await this.getActive(null, gameId);

    this.messenger.emit('game', game);
  }

  /**
   * Remove a player from a game
   */
  async playerPart(playerId)
  {
    const resp = await this.sql.query(`
        delete from game_players
        where player_id = @playerId
        returning game_id as id
        `, {playerId});

    const data = resp.firstOrNull();

    // player wasnt yet in a game
    if (!data) {
      return;
    }

    const {id} = data;

    // update status of game if active
    const game = await this.getActive(null, id);

    // no longer active
    if (!game) {
      // TODO: cleanup game?
      return;
    }

    // host left?
    if (game.hostPlayerId === playerId) {
      if (game.currentPlayerCount === 0) {
        await this.remove(id);
        return;
      }

      await this.assignHost(id);
      return;
    }

    // Otherwise just broadcast
    this._broadcastGameUpdate(game);
  }

  /**
   * Will blow up if theres still players in the game!
   */
  @hrtime('removed game in %s ms')
  async remove(id)
  {
    const resp = await this.sql.query(`
        delete from games where id = @id
        returning id`, {id});

    const data = resp.firstOrNull();

    if (!data || !data.id) {
      return;
    }
  }

  @hrtime('assigned new host in %s ms')
  async assignHost()
  {

  }

  /**
   * Create an entry for a game
   */
  @hrtime('created new game in %d ms')
  async create(name, componentId, hostId, maxPlayerCount = 2)
  {
    const resp = await this.sql.query(`

        insert into games
          (name, game_component_id, host_player_id, max_player_count)
        values
          (@name, @componentId, @hostId, @maxPlayerCount)
        returning id

      `,
      {name, componentId, hostId, maxPlayerCount});

    const {id} = resp.firstOrNull();

    // host always joins
    await this.playerJoin(hostId, id);

    // notify new game is out there
    const game = await this.getActive(null, id);
    this._broadcastGameUpdate(game);

    return game;
  }

  /**
   * Game is removed
   */
  async _broadcastRemoveGame(gameId)
  {
    await this.messenger.emit('game:remove', gameId);
  }

  /**
   * Broadcast game info
   */
  async _broadcastGameUpdate(game)
  {
    await this.messenger.emit('game', game);
  }
}
