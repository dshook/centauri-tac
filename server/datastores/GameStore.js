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
   * TODO: probably lots of optimization potential here with overselecting and nested queries
   */
  @hrtime('queried games in %s ms')
  async getActive(id = null)
  {
    let sql = `
      select
        g.*,
        s.*,
        (SELECT count(*) FROM game_players WHERE state = 1 AND game_id = g.id) AS current_player_count
      from games as g
      left join game_states s
        on g.game_state_id = s.id
    `;

    const params = {};

    if (id !== null) {
      sql += ' where g.id = @id ';
      params.id = id;
    }

    const models = [Game, GameState];

    const resp = await this.sql.tquery(...models)(sql, params,
      (g, gs, current_player_count) => {
        g.state = gs;
        g.currentPlayerCount = 0 | current_player_count;
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
        select game_id as id, state
        from game_players
        where player_id = @playerId
        and game_id = @gameId
        `, {playerId, gameId});

    let data = resp.firstOrNull();
    if (data) {
      if(data.state === 1){
        this.log.info('player %s already in game %s, not adding', playerId, gameId);
        return;
      }else{
        await this.sql.query(`
            update game_players set state = 1
            where player_id = @playerId
            and game_id = @gameId
            `, {playerId, gameId});
      }
    }

    await this.sql.query(`
        insert into game_players (player_id, game_id, state)
        values (@playerId, @gameId, 1)
        `, {playerId, gameId});
  }

  /**
   * Get current game id a player is in
   */
  async currentGameId(playerId)
  {
    const resp = await this.sql.query(`
        select game_id as id from game_players
        where player_id = @playerId and state = 1`,
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
    let gameId = await this.currentGameId(playerId);

    // player wasnt yet in a game, nothing to do
    if(!gameId){
      return null;
    }

    await this.sql.query(`
        update game_players set state = 0
        where player_id = @playerId
        and game_id = @gameId
        `, {playerId, gameId});

    return gameId;
  }

  /**
   * Will blow up if theres still players in the game!
   */
  @hrtime('removed game in %s ms')
  async complete(id, winningPlayerId)
  {
    const resp = await this.sql.query(`
        update games set
        game_state_id = 4,
        winning_player_id = @winningPlayerId
        where id = @id
        returning id`, {id, winningPlayerId});

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
        where gp.game_id = @gameId and state = 1
        `, {gameId});

    return resp.toArray();
  }

  /**
   * Create an entry for a game
   */
  @hrtime('created new game in %d ms')
  async create(name, map, maxPlayerCount, turnLengthMs, turnEndBufferLengthMs, turnIncrementLengthMs)
  {
    const resp = await this.sql.query(`

        insert into games
          (name, map, max_player_count, turn_length_ms, turn_end_buffer_ms, turn_increment_ms)
        values
          (@name, @map, @maxPlayerCount, @turnLengthMs, @turnEndBufferLengthMs, @turnIncrementLengthMs)
        returning id

      `,
      {name, map, maxPlayerCount, turnLengthMs, turnEndBufferLengthMs, turnIncrementLengthMs});

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
