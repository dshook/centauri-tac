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

  /**
   * Cast from data
   */
  static fromJSON(data)
  {
    if (data === null) {
      return null;
    }

    // dump
    return Object.assign(new GameState(), data);
  }
}
