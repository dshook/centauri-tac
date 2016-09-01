import BaseAction from './BaseAction.js';

//Sends a string message to the client to display
export default class Message extends BaseAction
{
  constructor(msg)
  {
    super();
    this.message = msg;
  }
}
