import moment from 'moment';
import fromSQL from 'postgrizzly/fromSQLSymbol';
import GameState from './GameState.js';

/**
 * A game.
 */
export default class Game
{
  constructor()
  {
    this.id = null;
    this.name = null;

    this.map = null;

    this.maxPlayerCount = null;

    this.registered = null;

    this.stateId = null;
    this.state = null;

    this.turnLengthMs = null;
    this.turnEndBufferLengthMs = null;
    this.turnIncrementLengthMs = null;

    this.allowJoin = true;
    this.allowCommands = false; //Accept commands from players

    this.winningPlayerId = null;
  }

  /**
   * From DB
   */
  static [fromSQL](resp)
  {
    if (resp.id === null) {
      return null;
    }

    const g = new Game();
    g.id = resp.id;
    g.name = resp.name;
    g.map = resp.map;
    g.maxPlayerCount = resp['max_player_count'];
    g.registered = resp.registered;
    g.stateId = resp['game_state_id'];
    g.allowJoin = !!resp['allow_join'];
    g.turnLengthMs = resp['turn_length_ms'];
    g.turnEndBufferLengthMs = resp['turn_end_buffer_ms'];
    g.turnIncrementLengthMs = resp['turn_increment_ms'];
    g.winningPlayerId = resp['winning_player_id'];
    return g;
  }

  /**
   * From endpoint
   */
  static fromJSON(data)
  {
    if (!data) {
      return null;
    }

    const g = new Game();
    Object.assign(g, data);
    g.registered = moment.parseZone(data.registered);
    g.state = GameState.fromJSON(data.state);

    return g;
  }

  /**
   * Output to endpoint
   */
  toJSON()
  {
    return {
      ...this,
      registered: this.registered.format(),
    };
  }
}
