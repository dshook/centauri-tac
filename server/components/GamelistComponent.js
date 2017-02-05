import loglevel from 'loglevel-decorator';
import GamelistRPC from '../api/GamelistRPC.js';
import GamelistAPI from '../api/GamelistAPI.js';

const CLEANUP_INTERVAL = 120 * 1000;

/**
 * Game server component that provides lists of running games
 */
@loglevel
export default class GamelistComponent
{
  constructor(games)
  {
    this.games = games;
    setInterval(() => this.cleanup(), CLEANUP_INTERVAL);
  }

  /**
   * Boot component
   */
  async start(component)
  {
    const sock = component.sockServer;
    const rest = component.restServer;

    await this.cleanup();
    sock.addHandler(GamelistRPC);
    rest.mountController('/game', GamelistAPI);
  }

  /**
   * Clean up DB, TODO: figure out if anything is needed now
   */
  async cleanup()
  {
  }
}
