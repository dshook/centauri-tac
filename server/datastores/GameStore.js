import loglevel from 'loglevel-decorator';
import Game from 'models/Game';
import Component from 'models/Component';
import ComponentType from 'models/ComponentType';
import Player from 'models/Player';
import GameState from 'models/GameState';

/**
 * Data layer for games
 */
@loglevel
export default class GameStore
{
  constructor(sql, players)
  {
    this.sql = sql;
    this.players = players;
  }

  /**
   * Every damn game (or by realm)
   */
  async all(realm = null)
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
      left join (select game_id, count(*) as current_player_count
          from game_players
          group by game_id) as counts
        on g.id = counts.game_id

    `;

    const params = {};

    // TODO: active check here
    if (realm) {
      sql += `
        where c.realm = @realm
      `;

      params.realm = realm;
    }

    const models = [Game, Component, ComponentType, Player, GameState];

    const resp = await this.sql.tquery(...models)(sql, params,
      (g, c, t, p, gs, cpc) => {
        g.gameComponent = c;
        g.gameComponent.type = t;
        g.hostPlayer = p;
        g.state = gs;
        g.currentPlayerCount = 0 | cpc['current_player_count'];
        return g;
      });

    return resp.toArray();
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
    await this.sql.query(`
        insert into game_players (player_id, game_id)
        values (@playerId, @gameId)
        `, {playerId, gameId});
  }

  /**
   * Remove a player from a game
   */
  async playerPart(playerId, gameId)
  {
    await this.sql.query(`
        delete from game_players
        where player_id = @playerId
        and game_id = @gameId`,
        {playerId, gameId});
  }

  /**
   * Get active games from a particular realm
   */
  async activeFromRealm(realm)
  {
    return await this.all(realm);
  }

  /**
   * Create a new game entry from a model, updates the id in the model
   */
  async register(game: Game): Promise<Game>
  {
    const resp = await this.sql.query(Game)(`

      insert into games
        (name, game_component_id, host_player_id)
      values
        (@name, @componentId, @hostPlayerId)
      returning id

      `,
      game);

    const {id} = resp.firstOrNull();

    game.id = id;

    return game;
  }
}
