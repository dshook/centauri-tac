import BaseAction from './BaseAction.js';

//Queue this to manually update auras in the middle of the queue, like for move
export default class UpdateAura extends BaseAction
{
  constructor()
  {
    super();
    this.serverOnly = true;
  }
}
