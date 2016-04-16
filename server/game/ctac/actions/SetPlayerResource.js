export default class SetPlayerResource
{
  constructor(playerId, change, permanent = false, filled = true)
  {
    this.playerId = playerId;
    this.change = change;
    this.permanent = permanent;
    this.filled = filled;

    this.newAmount = null;
    this.newMax = null;
  }
}
