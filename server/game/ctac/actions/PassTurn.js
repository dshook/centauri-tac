/**
 * An action that passes the turn to a player
 */
export default class PassTurn
{
  constructor(to, from = null)
  {
    this.id = null;
    this.to = to;
    this.from = from;
  }
}
