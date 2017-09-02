import GameManager from '../game/GameManager.js';

/**
 * Manage running games
 */
export default class GameService
{
  constructor(container)
  {
    container.registerSingleton('gameManager', GameManager);
  }
}
