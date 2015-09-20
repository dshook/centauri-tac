import GameManager from '../game/GameManager.js';
import GameInstance from '../game/GameInstance.js';

/**
 * Manage running games
 */
export default class GameService
{
  constructor(app)
  {
    app.registerInstance('gameInstanceFactory', () => app.make(GameInstance));
    app.registerSingleton('gameManager', GameManager);
  }
}
