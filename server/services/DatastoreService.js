import PlayerStore from '../datastores/PlayerStore.js';
import GameStore from '../datastores/GameStore.js';
import DeckStore from '../datastores/DeckStore.js';

/**
 * Store abstractions on top of our data layer
 */
export default class DatastoreService
{
  constructor(container)
  {
    container.registerSingleton('players', PlayerStore);
    container.registerSingleton('games', GameStore);
    container.registerSingleton('decks', DeckStore);
  }
}
