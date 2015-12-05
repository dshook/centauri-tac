import TurnState from '../models/TurnState.js';
import PlayerResourceState from '../models/PlayerResourceState.js';
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
    app.registerInstance('turnState', this.turnState);
    app.registerInstance('playerResourceState', this.playerResourceState);

    this.pieceState = new PieceState();
    app.registerInstance('pieceState', this.pieceState);
  }
}
