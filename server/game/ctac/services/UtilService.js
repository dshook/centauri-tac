import loglevel from 'loglevel-decorator';

/**
 * Service for things that don't quite fit elsewhere
 */
@loglevel
export default class UtilService
{
  constructor(app, queue)
  {
    app.registerInstance('possibleActions', app.make(PossibleActions));
  }
}

class PossibleActions
{
  constructor(cardEvaluator, cardState, turnState, pieceState){
    this.cardEvaluator = cardEvaluator;
    this.cardState = cardState;
    this.turnState = turnState;
    this.pieceState = pieceState;
  }

  //look through the current players hand for any cards needing a TARGET
  //and through pieces for possible abilities
  findPossibleActions(){
    let targets = this.cardEvaluator.findPossibleTargets(
      this.cardState.hands[this.turnState.currentPlayerId],
      this.turnState.currentPlayerId
    );
    let abilities = this.cardEvaluator.findPossibleAbilities(
      this.pieceState.pieces,
      this.turnState.currentPlayerId
    );
    return {
      playerId: this.turnState.currentPlayerId,
      targets,
      abilities
    };
  }
}