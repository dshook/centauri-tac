import Matchmaker from '../game/Matchmaker.js';
import CardManager from '../game/CardManager.js';
import CardDirectory from '../game/ctac/models/CardDirectory.js';
import GameConfig from '../game/GameConfig.js';

/**
 * Expose the matchmaker
 */
export default class LobbyService
{
  constructor(container)
  {
    container.registerSingleton('gameConfig', GameConfig);
    container.registerSingleton('cardDirectory', CardDirectory);
    container.registerSingleton('matchmaker', Matchmaker);
    container.registerSingleton('cardManager', CardManager);
  }
}
