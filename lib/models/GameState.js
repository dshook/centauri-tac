export default class GameState
{
  constructor()
  {
    this.id = null;
    this.name = null;
  }

  static fromJSON(data)
  {
    if (data === null) {
      return null;
    }

    const gs = new GameState();
    gs.id = data.id;
    gs.name = data.name;

    return gs;
  }
}
