import loglevel from 'loglevel-decorator';
import GameAPI from '../api/GameAPI.js';

/**
 * Game server component that provides lists of running games
 */
@loglevel
export default class GamelistComponent
{
  constructor(app)
  {
    // TODO: move the features of this stuff into a game manager type thing
    app.registerSingleton('gamelistComponent', this);
  }

  async start(server, rest)
  {
    rest.mountController('/game', GameAPI);
  }

  /**
   * Function that will find a component to host a game on and instruct it to
   * spin up a new instnace
   */
  async getComponentToHostGame()
  {
    return 60;
  }
}
