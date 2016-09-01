import BaseAction from './BaseAction.js';

//Sends a string message to the client to display
export default class SendMessage extends BaseAction
{
  constructor(msg)
  {
    super();
    this.message = msg;
  }
}
