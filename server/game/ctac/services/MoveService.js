import loglevel from 'loglevel-decorator';
import MoveProcessor from '../processors/MovePieceProcessor.js';

@loglevel
export default class MoveService
{
  constructor(app, queue)
  {
    queue.addProcessor(MoveProcessor);
  }
}
