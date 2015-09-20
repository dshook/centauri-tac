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
    const gId = await this.currentGameId(playerId);

    // if player is already in a game, just barf
    if (gId) {
      throw new Error('Player is already in a game');
    }

    await this.sql.query(`
        insert into game_players (player_id, game_id)
        values (@playerId, @gameId)
        `, {playerId, gameId});

    const game = await this.getActive(null, gameId);

    await this.messenger.emit('game:current', {game, playerId});
    await this.messenger.emit('game', game);
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
      return;
    }
    await this.messenger.emit('game:current', {game: null, playerId});

    const {id} = data;
    const game = await this.getActive(null, id);

    // no longer active...
    if (!game) {
      await this._handleZomieGame(id);
      return;
    }
    else if (game.hostPlayerId === playerId) {
      await this._handlePlayerWasHostAndLeft(game);
      return;
    }

    // Otherwise just broadcast updated model
    await this.messenger.emit('game', game);
  }

  /**
   * Player is listed as being in a game no longer on a running server
   */
  async _handleZomieGame(gameId)
  {
    this.log.info('player was in zombie game %s...', gameId);
    this.log.info('TODO: drop any remaining players and remove game');
  }

  /**
   * Player was the host of game but parted it
   */
  async _handlePlayerWasHostAndLeft(game)
  {
    this.log.info('host left');

    // game was total empty, time to remove it
    if (game.currentPlayerCount === 0) {
      this.log.info('...and was last one');

      // and never started by teh game server
      if (game.state === null) {
        this.log.info('...and was never started on the server');
      }
      else {
        this.log.info('TODO: cleanup game server');
      }

      await this.remove(game.id);
      return;
    }

    // not empty yet
    await this.assignHost(game.id);
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

    await this.messenger.emit('game:remove', id);
  }

  /**
   * Give host to another player in a game. Assumes game isnt empty
   */
  @hrtime('assigned new host in %s ms')
  async assignHost(gameId)
  {
    const resp = await this.sql.query(`
        select player_id as id from game_players
        where game_id = @gameId
        `, {gameId});

    const playerId = resp.firstOrNull().id;

    this.log.info('giving player %s host for game %s', playerId, gameId);

    await this.sql.query(`
        update games set host_player_id = @playerId
        where id = @gameId
        `, {playerId, gameId});

    // broadcast updated game info
    await this.messenger.emit('game', await this.getActive(null, gameId));
  }

  /**
   * Remove players from dead games
   */
  @hrtime('removed zombie game players in %d ms')
  async removeZombieGamePlayers()
  {
    const resp = await this.sql.query(`

        delete from game_players
        where game_id in
          (select g.id from games g
            join components c
              on g.game_component_id = c.id
            where not c.is_active)
        returning game_id as id

    `);

    this.log.info('cleaned up %s zombie game players', resp.toArray().length);
  }

  /**
   * Remove all empty games on dead compnoents
   */
  @hrtime('removed zomie games in %d ms')
  async removeZombieGames()
  {
    const resp = await this.sql.query(`

        delete from games
        where game_component_id in
          (select id from components c where not c.is_active)
        and id not in
          (select distinct game_id as id from game_players)
        returning id

    `);

    const ids = resp.toArray();

    this.log.info('cleaned up %s empty zombie games', ids.length);
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

    const game = await this.getActive(null, id);
    return game;
  }
}
