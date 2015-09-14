import moment from 'moment';
import fromSQL from 'postgrizzly/fromSQLSymbol';
import Component from './Component.js';
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
    this.currentPlayerCount = null;
    this.gameComponentId = null;
    this.gameComponent = null;
    this.hostPlayerId = null;
    this.hostPlayer = null;
    this.registered = null;
    this.lastPing = null;
    this.stateId = null;
    this.state = null;
  }

  static [fromSQL](resp)
  {
    if (resp.id === null) {
      return null;
    }

    const g = new Game();
    g.id = resp.id;
    g.name = resp.name;
    g.hostPlayerId = resp['host_player_id'];
    g.gameComponentId = resp['game_component_id'];
    g.maxPlayerCount = resp['max_player_count'];
    g.currentPlayerCount = resp['current_player_count'];
    g.registered = resp.registered;
    g.lastPing = resp['last_ping'];
    g.stateId = resp['game_state_id'];
    return g;
  }

  /**
   * From endpoint
   */
  static fromJSON(data)
  {
    const g = new Game();
    Object.assign(g, data);
    g.registered = moment.parseZone(data.registered);
    g.lastPing = data.lastPing ? moment.parseZone(data.lastPing) : null;
    g.gameComponent = Component.fromJSON(data.gameComponent);
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
      lastPing: this.lastPing ? this.lastPing.format() : null,
    };
  }
}
