import loglevel from 'loglevel-decorator';
import requireDir from 'require-dir';

import UpdateAuraProcessor from '../processors/UpdateAuraProcessor.js';

/**
 * Add action processors to the queue
 */
@loglevel
export default class TurnService
{
  constructor(app, queue)
  {
    var processors = requireDir('../processors/');
    for(let processor in processors){
      if(processor === 'NoOpProcessor' || processor === 'UpdateAuraProcessor'){
        continue;
      }
      let ctor = processors[processor].default;
      queue.addProcessor(ctor);
      //this.log.info('Registered processor %s type %s', processor, typeof ctor);
    }

    //add update aura to the end of all process complete
    queue.addPostCompleteProcessor(UpdateAuraProcessor);
  }
}
