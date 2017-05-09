import GamelistManager from '../game/GamelistManager.js';
import Matchmaker from '../game/Matchmaker.js';

/**
 * Expose the gamelist manager
 */
export default class GamelistService
{
  constructor(container)
  {
    container.registerSingleton('gamelistManager', GamelistManager);
    container.registerSingleton('matchmaker', Matchmaker);
  }
}
