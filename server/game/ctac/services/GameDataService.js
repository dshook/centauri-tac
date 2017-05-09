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
  constructor(app, queue)
  {
    this.turnState = new TurnState();
    this.playerResourceState = new PlayerResourceState();
    this.statsState = new StatsState();
    app.container.registerValue('turnState', this.turnState);
    app.container.registerValue('playerResourceState', this.playerResourceState);
    app.container.registerValue('statsState', this.statsState);

    this.pieceState = new PieceState();
    app.container.registerValue('pieceState', this.pieceState);
  }
}
