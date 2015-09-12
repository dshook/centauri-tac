import {route} from 'http-tools';

/**
 * REST API for dealing with Game models
 */
export default class GameAPI
{
  constructor(games)
  {
    this.games = games;
  }

  @route.get('/')
  async getAllGames()
  {
    return [ ];
  }
}
