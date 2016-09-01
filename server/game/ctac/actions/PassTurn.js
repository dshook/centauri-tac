import BaseAction from './BaseAction.js';

/**
 * An action that passes the turn to a player
 */
export default class PassTurn extends BaseAction
{
  constructor(to, from = null)
  {
    super();
    this.to = to;
    this.from = from;
    this.currentTurn = null;
    this.toPlayerResources = null;
    this.toPlayerMaxResources = null;
  }
}
