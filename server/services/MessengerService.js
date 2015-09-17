import Messenger from '../components/Messenger.js';

export default class MessengerService
{
  constructor(app)
  {
    app.registerSingleton('messenger', Messenger);
  }
}
