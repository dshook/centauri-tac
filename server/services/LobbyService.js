import Matchmaker from '../game/Matchmaker.js';
import CardManager from '../game/CardManager.js';

/**
 * Expose the matchmaker
 */
export default class LobbyService
{
  constructor(container)
  {
    container.registerSingleton('matchmaker', Matchmaker);
    container.registerSingleton('cardManager', CardManager);
  }
}
