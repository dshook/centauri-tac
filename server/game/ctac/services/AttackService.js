import loglevel from 'loglevel-decorator';
import AttackProcessor from '../processors/AttackPieceProcessor.js';
import HealthChangeProcessor from '../processors/HealthChangeProcessor.js';

@loglevel
export default class AttackService
{
  constructor(app, queue)
  {
    queue.addProcessor(AttackProcessor);
    queue.addProcessor(HealthChangeProcessor);
  }
}
