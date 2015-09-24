import GameManager from '../game/GameManager.js';
import CentauriTacGame from '../game/ctac/CentauriTacGame.js';

/**
 * Manage running games
 */
export default class GameService
{
  constructor(app)
  {
    // All of the services booted up for each new game
    app.registerInstance('gameModules', {

      ctact: CentauriTacGame,

    });

    app.registerSingleton('gameManager', GameManager);
  }
}
