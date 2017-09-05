import BaseAction from './BaseAction.js';

//Sends a message to the client telling them the game kicked off
export default class Kickoff extends BaseAction
{
  constructor(msg)
  {
    super();
    this.message = msg;
  }
}
