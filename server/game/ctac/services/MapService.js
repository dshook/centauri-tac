import loglevel from 'loglevel-decorator';
import MapState from '../models/MapState.js';
import requireDir from 'require-dir';

/**
 * Expose the cards and activate card processor
 */
@loglevel
export default class MapService
{
  constructor(container, queue)
  {
    var mapRequires = requireDir('../../../../maps/');
    var mapState = container.new(MapState);

    for(let mapName in mapRequires){
      let map = mapRequires[mapName];
      mapState.add(map);
      this.log.info('Registered map %s', mapName);
    }
    var mapState = container.registerValue('mapState', mapState);
  }
}
