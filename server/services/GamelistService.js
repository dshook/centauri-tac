import GamelistManager from '../game/GamelistManager.js';

/**
 * Expose the gamelist manager
 */
export default class GamelistService
{
  constructor(app)
  {
    app.registerSingleton('gamelistManager', GamelistManager);
  }
}
