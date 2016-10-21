import BaseAction from './BaseAction.js';

export default class ChargeUp extends BaseAction
{
  constructor(playerId, change)
  {
    super();
    this.playerId = playerId;
    this.change = change;
    this.charge = null;
  }
}
