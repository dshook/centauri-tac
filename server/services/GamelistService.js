import GamelistManager from '../game/GamelistManager.js';
import Matchmaker from '../game/Matchmaker.js';

/**
 * Expose the gamelist manager
 */
export default class GamelistService
{
  constructor(app)
  {
    app.registerSingleton('gamelistManager', GamelistManager);
    app.registerSingleton('matchmaker', Matchmaker);
  }
}
