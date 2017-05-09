import TurnState from '../models/TurnState.js';
import PlayerResourceState from '../models/PlayerResourceState.js';
import StatsState from '../models/StatsState.js';
import PieceState from '../models/PieceState.js';
import loglevel from 'loglevel-decorator';

/**
 * Initialize and wire up all the game data models
 */
@loglevel
export default class TurnService
{
  constructor(container, queue)
  {
    this.turnState = new TurnState();
    this.playerResourceState = new PlayerResourceState();
    this.statsState = new StatsState();
    container.registerValue('turnState', this.turnState);
    container.registerValue('playerResourceState', this.playerResourceState);
    container.registerValue('statsState', this.statsState);

    this.pieceState = new PieceState();
    container.registerValue('pieceState', this.pieceState);
  }
}
