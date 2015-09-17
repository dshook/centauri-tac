import fromSQL from 'postgrizzly/fromSQLSymbol';

/**
 * Current state a game is in
 */
export default class GameState
{
  constructor()
  {
    this.id = null;
    this.name = null;
  }

  static [fromSQL](data)
  {
    if (!data || !data.id) {
      return null;
    }

    // dump
    return Object.assign(new GameState(), data);
  }

  /**
   * Cast from data
   */
  static fromJSON(data)
  {
    if (!data || !data.id) {
      return null;
    }

    // dump
    return Object.assign(new GameState(), data);
  }
}
