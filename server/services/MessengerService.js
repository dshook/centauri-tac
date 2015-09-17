import Messenger from 'rpc-messenger';

export default class MessengerService
{
  constructor(app)
  {
    app.registerSingleton('messenger', Messenger);
  }
}
