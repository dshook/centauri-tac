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

    this.maxPlayerCount = null;

    this.hostPlayerId = null;
    this.hostPlayer = null;

    this.registered = null;

    this.stateId = null;
    this.state = null;

    this.turnLengthMs = null;

    this.allowJoin = true;
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
    g.hostPlayerId = resp['host_player_id'];
    g.maxPlayerCount = resp['max_player_count'];
    g.registered = resp.registered;
    g.stateId = resp['game_state_id'];
    g.allowJoin = !!resp['allow_join'];
    g.turnLengthMs = resp['turn_length_ms'];
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
