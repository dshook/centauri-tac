import ComponentStore from '../datastores/ComponentStore.js';
import PlayerStore from '../datastores/PlayerStore.js';
import GameStore from '../datastores/GameStore.js';

/**
 * Store abstractions on top of our data layer
 */
export default class DatastoreService
{
  constructor(app)
  {
    app.registerSingleton('components', ComponentStore);
    app.registerSingleton('players', PlayerStore);
    app.registerSingleton('games', GameStore);
  }
}
