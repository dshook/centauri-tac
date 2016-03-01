import loglevel from 'loglevel-decorator';
import TurnProcessor from '../processors/TurnProcessor.js';
import PlayerResourceProcessor from '../processors/PlayerResourceProcessor.js';
import MoveProcessor from '../processors/MovePieceProcessor.js';
import AttackProcessor from '../processors/AttackPieceProcessor.js';
import HealthChangeProcessor from '../processors/HealthChangeProcessor.js';
import PieceAttributeChangeProcessor from '../processors/PieceAttributeChangeProcessor.js';
import PieceBuffProcessor from '../processors/PieceBuffProcessor.js';
import SpawnPieceProcessor from '../processors/SpawnPieceProcessor.js';
import ActivateCardProcessor from '../processors/ActivateCardProcessor.js';
import CardDrawProcessor from '../processors/CardDrawProcessor.js';
import SpawnDeckProcessor from '../processors/SpawnDeckProcessor.js';
import PlaySpellProcessor from '../processors/PlaySpellProcessor.js';
import RotatePieceProcessor from '../processors/RotatePieceProcessor.js';

/**
 * Add action processors to the queue
 */
@loglevel
export default class TurnService
{
  constructor(app, queue)
  {
    queue.addProcessor(TurnProcessor);
    queue.addProcessor(PlayerResourceProcessor);
    queue.addProcessor(MoveProcessor);
    queue.addProcessor(AttackProcessor);
    queue.addProcessor(HealthChangeProcessor);
    queue.addProcessor(PieceAttributeChangeProcessor);
    queue.addProcessor(PieceBuffProcessor);
    queue.addProcessor(SpawnPieceProcessor);

    queue.addProcessor(SpawnDeckProcessor);
    queue.addProcessor(ActivateCardProcessor);
    queue.addProcessor(CardDrawProcessor);
    queue.addProcessor(PlaySpellProcessor);
    queue.addProcessor(RotatePieceProcessor);
  }
}
