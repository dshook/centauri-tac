import loglevel from 'loglevel-decorator';
import GamelistRPC from '../api/GamelistRPC.js';

const CLEANUP_INTERVAL = 30 * 1000;

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

    //await this.cleanup();
    sock.addHandler(GamelistRPC);
  }

  /**
   * Clean up DB, TODO: figure out if anything is needed now
   */
  async cleanup()
  {
    this.log.info('Cleanup up Games');
    this.games.cleanupGames();
  }
}
