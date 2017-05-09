import GameManager from '../game/GameManager.js';
import CentauriTacGame from '../game/ctac/CentauriTacGame.js';

/**
 * Manage running games
 */
export default class GameService
{
  constructor(container)
  {
    // All of the services booted up for each new game
    container.registerValue('gameModules', {

      ctact: CentauriTacGame,

    });

    container.registerSingleton('gameManager', GameManager);
  }
}
