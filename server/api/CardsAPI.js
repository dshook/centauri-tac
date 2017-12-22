import {route} from 'http-tools';
import {HttpError} from 'http-tools';
import GameConfig from '../game/GameConfig.js';
import CardDirectory from '../game/ctac/models/CardDirectory.js';

/**
 * REST API for client to fetch card directory from
 */
export default class CardsAPI
{
  constructor(auth, players)
  {
    this.players = players;
    this.auth = auth;
    this.gameConfig = new GameConfig();
    this.cardDirectory = new CardDirectory(this.gameConfig);
  }

  /**
   * /components/game/rest/cards/directory
   */
  @route.get('/directory')
  async directory(req)
  {
    try {
      await new Promise(res => {
        setTimeout(() => res(), 2000);
      })
      return this.cardDirectory.rawDirectory;
    }
    catch (err) {

      // shit went wrong
      throw new HttpError(400, err.message);
    }
  }
}

