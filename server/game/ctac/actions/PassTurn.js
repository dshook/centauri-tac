import BaseAction from './BaseAction.js';

/**
 * An action that passes the turn to a player
 */
export default class PassTurn extends BaseAction
{
  constructor()
  {
    super();
    this.currentTurn = null;
    this.playerResources = [];
  }
}
