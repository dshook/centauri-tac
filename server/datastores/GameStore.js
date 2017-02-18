import loglevel from 'loglevel-decorator';
import Game from 'models/Game';
import Player from 'models/Player';
import GameState from 'models/GameState';
import hrtime from 'hrtime-log-decorator';

/**
 * Data layer for games
 */
@loglevel
export default class GameStore
{
  constructor(sql)
  {
    this.sql = sql;
  }

  /**
   * Active games on (or a single one)
   */
  @hrtime('queried games in %s ms')
  async getActive(id = null)
  {
    let sql = `

      select g.*, p.*, s.*, counts.current_player_count
      from games as g
      join players p
        on g.host_player_id = p.id
      left join game_states s
        on g.game_state_id = s.id

      -- show curent player count
      left join (select game_id, count(*) as current_player_count
          from game_players
          group by game_id) as counts
        on g.id = counts.game_id

    `;

    const params = {};

    if (id !== null) {
      sql += ' and g.id = @id ';
      params.id = id;
    }

    const models = [Game, Player, GameState];

    const resp = await this.sql.tquery(...models)(sql, params,
      (g, p, gs, cpc) => {
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
    const resp = await this.sql.query(`
        select game_id as id
        from game_players
        where player_id = @playerId
        and game_id = @gameId
        `, {playerId, gameId});

    if (resp.firstOrNull()) {
      this.log.info('player %s already in game %s, not adding', playerId, gameId);
      return;
    }

    await this.sql.query(`
        insert into game_players (player_id, game_id)
        values (@playerId, @gameId)
        `, {playerId, gameId});
  }

  /**
   * Get current game id a player is ine
   */
  async currentGameId(playerId)
  {
    const resp = await this.sql.query(`
        select game_id as id from game_players
        where player_id = @playerId`,
        {playerId});

    const data = resp.firstOrNull();

    if (!data) {
      return null;
    }

    return data.id;
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

    // player wasnt yet in a game, nothing to do
    if (!data) {
      return null;
    }

    return data.id;
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

  /**
   * Allow join flag
   */
  @hrtime('set allow join in %s ms')
  async setAllowJoin(gameId, allowJoin = true)
  {
    await this.sql.query(`
        update games
        set allow_join = @allowJoin
        where id = @gameId
        `, {gameId, allowJoin});
  }

  /**
   * Change run state of a game
   */
  @hrtime('updated game state in %s ms')
  async setState(gameId, stateId)
  {
    await this.sql.query(`
        update games
        set game_state_id = @stateId
        where id = @gameId
        `, {gameId, stateId});
  }

  /**
   * Get all players currently in a game
   */
  async playersInGame(gameId)
  {
    const resp = await this.sql.tquery(Player)(`
        select p.*
        from game_players gp
        join players p on gp.player_id = p.id
        where gp.game_id = @gameId
        `, {gameId});

    return resp.toArray();
  }

  /**
   * Update the host of a game
   */
  async setHost(gameId, playerId)
  {
    await this.sql.query(`
        update games
        set host_player_id = @playerId
        where id = @gameId
        `, {playerId, gameId});
  }

  /**
   * Create an entry for a game
   */
  @hrtime('created new game in %d ms')
  async create(name, hostId, maxPlayerCount = 2, turnLengthMs = 30000)
  {
    const resp = await this.sql.query(`

        insert into games
          (name, host_player_id, max_player_count, turn_length_ms)
        values
          (@name, @hostId, @maxPlayerCount, @turnLengthMs)
        returning id

      `,
      {name, hostId, maxPlayerCount, turnLengthMs});

    const {id} = resp.firstOrNull();

    const game = await this.getActive(id);
    return game;
  }

  /**
   * Clean up zombie games hanging around with no players in them
   */
  async cleanupGames()
  {
    await this.sql.query(`
      DELETE
      FROM games G
      WHERE (SELECT count(*) FROM game_players GP WHERE G.id = GP.game_id) = 0
        AND G.registered < NOW() - INTERVAL '5 minutes'
    `);
  }
}
