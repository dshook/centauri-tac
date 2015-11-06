export default class SetPlayerResource
{
  constructor(playerId, change)
  {
    this.playerId = playerId;
    this.change = change;
    this.newTotal = null;
  }
}
