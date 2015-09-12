import moment from 'moment';
import fromSQL from 'postgrizzly/fromSQLSymbol';
import Component from './Component.js';

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
