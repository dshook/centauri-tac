import ComponentStore from '../datastores/ComponentStore.js';
import PlayerStore from '../datastores/PlayerStore.js';
import loglevel from 'loglevel-decorator';

/**
 * Store abstractions on top of our data layer
 */
@loglevel
export default class DatastoreService
{
  constructor(app)
  {
    app.registerSingleton('components', ComponentStore);
    app.registerSingleton('players', PlayerStore);

    this.log.info('registered all datastores');
  }
}
