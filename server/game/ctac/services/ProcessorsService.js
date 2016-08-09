import loglevel from 'loglevel-decorator';
import requireDir from 'require-dir';

import UpdateAuraProcessor from '../processors/UpdateAuraProcessor.js';
import UpdateBuffsProcessor from '../processors/UpdateBuffsProcessor.js';

/**
 * Add action processors to the queue
 */
@loglevel
export default class ProcessorsService
{
  constructor(app, queue)
  {
    var processors = requireDir('../processors/');
    let skip = ['NoOpProcessor', 'UpdateAuraProcessor', 'UpdateBuffsProcessor'];
    for(let processor in processors){
      if(skip.includes(processor)) continue;

      let ctor = processors[processor].default;
      queue.addProcessor(ctor);
      //this.log.info('Registered processor %s type %s', processor, typeof ctor);
    }

    //update buffs and auras after queue is complete
    queue.addPostCompleteProcessor(UpdateBuffsProcessor);
    queue.addPostCompleteProcessor(UpdateAuraProcessor);
  }
}
