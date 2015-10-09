import loglevel from 'loglevel-decorator';
import AttackProcessor from '../processors/AttackPieceProcessor.js';

@loglevel
export default class AttackService
{
  constructor(app, queue)
  {
    queue.addProcessor(AttackProcessor);
  }
}
