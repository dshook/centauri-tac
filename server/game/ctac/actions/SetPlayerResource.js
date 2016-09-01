import BaseAction from './BaseAction.js';

export default class SetPlayerResource extends BaseAction
{
  constructor(playerId, change, permanent = false, filled = true)
  {
    super();
    this.playerId = playerId;
    this.change = change;
    this.permanent = permanent;
    this.filled = filled;

    this.newAmount = null;
    this.newMax = null;
  }
}
