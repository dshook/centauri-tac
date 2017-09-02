import Matchmaker from '../game/Matchmaker.js';

/**
 * Expose the matchmaker
 */
export default class MatchmakerService
{
  constructor(container)
  {
    container.registerSingleton('matchmaker', Matchmaker);
  }
}
