import GameManager from '../game/GameManager.js';
import ChatDemo from '../game/ctac/ChatDemo.js';

/**
 * Manage running games
 */
export default class GameService
{
  constructor(app)
  {
    // All of the services booted up for each new game
    app.registerInstance('gameModules', {

      chat: ChatDemo,

    });

    app.registerSingleton('gameManager', GameManager);
  }
}
