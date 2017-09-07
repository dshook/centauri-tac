import Matchmaker from '../game/Matchmaker.js';

/**
 * Expose the matchmaker
 */
export default class LobbyService
{
  constructor(container)
  {
    container.registerSingleton('matchmaker', Matchmaker);
  }
}
