import loglevel from 'loglevel-decorator';
import Game from 'models/Game';
import Component from 'models/Component';
import ComponentType from 'models/ComponentType';
import Player from 'models/Player';
import GameState from 'models/GameState';
import {EventEmitter} from 'events';

/**
 * Data layer for games
 */
@loglevel
export default class GameStore extends EventEmitter
{
  constructor(sql, players)
  {
    super();
    this.sql = sql;
    this.players = players;
  }

  /**
   * Active games on realm
   */
  async getActive(realm)
  {
    if (!realm) {
      throw new TypeError('must specify realm');
    }

    const sql = `

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

      where c.realm = @realm
      and c.is_active

    `;

    const params = {realm};

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

    return resp.toArray();
  }

  /**
   * Get a (minimal model) game that the player is currently in
   */
  async playersCurrentGame(playerId)
  {
    const resp = await this.sql.tquery(Game, Component)(`

      select g.*, c.*
      from game_players gp
      join games as g on gp.game_id = g.id
      join components as c on g.game_component_id = c.id
      where gp.player_id = @playerId

    `, {playerId}, (g, c) => { g.component = c; return g; });

    const game = resp.firstOrNull();

    // cleanup after stale games / DCs / server restarts
    if (game && !game.component.isActive) {
      this.log.info('dropping player %s from stale game %s on %s',
          playerId, game.id, game.component.id);
      await this.playerPart(playerId);
      return null;
    }

    return game;
  }

  /**
   * Get all players in a game by id
   */
  async playersInGame(id)
  {
    const resp = await this.sql.tquery(Player)(`

        select p.*
        from game_players as gp
        join players p
          on gp.player_id = p.id
        where gp.game_id = @id

      `, {id});

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
  }

  /**
   * Remove a player from a game
   */
  async playerPart(playerId)
  {
    await this.sql.query(`
        delete from game_players
        where player_id = @playerId
        `, {playerId});
  }

  /**
   * Create an entry for a game
   */
  async create(name, componentId, hostId)
  {
    const resp = await this.sql.tquery(Game)(`

        insert into games
          (name, game_component_id, host_player_id)
        values
          (@name, @componentId, @hostId)
        returning *
      `, {name, componentId, hostId});


    const game = resp.firstOrNull();

    // host always joins
    await this.playerJoin(hostId, game.id);

    // notify new game is out there
    this.emit('game', game);

    return game;
  }
}
