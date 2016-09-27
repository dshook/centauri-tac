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
  constructor(cardEvaluator, cardState, turnState, pieceState, cardDirectory){
    this.cardEvaluator = cardEvaluator;
    this.cardState = cardState;
    this.turnState = turnState;
    this.pieceState = pieceState;
    this.cardDirectory = cardDirectory;
  }

  //end of processing client data update
  findPossibleActions(){
    //look through the current players hand for any cards needing a TARGET
    //and through pieces for possible abilities
    let targets = this.cardEvaluator.findPossibleTargets(
      this.cardState.hands[this.turnState.currentPlayerId],
      this.turnState.currentPlayerId
    );
    let abilities = this.cardEvaluator.findPossibleAbilities(
      this.pieceState.pieces,
      this.turnState.currentPlayerId
    );
    let areas = this.cardEvaluator.findPossibleAreas(
      this.cardState.hands[this.turnState.currentPlayerId],
      this.turnState.currentPlayerId
    );
    let metConditions = this.cardEvaluator.findMetConditionCards(
      this.cardState.hands[this.turnState.currentPlayerId],
      this.turnState.currentPlayerId
    );
    let eventedPieces = this.cardEvaluator.findEventedPieces();
    let chooseCards = this.cardEvaluator.findChooseCards(
      this.cardState.hands[this.turnState.currentPlayerId],
      this.turnState.currentPlayerId,
      this.cardDirectory
    );
    return {
      playerId: this.turnState.currentPlayerId,
      spellDamage: this.pieceState.totalSpellDamage(this.turnState.currentPlayerId),
      targets,
      areas,
      abilities,
      eventedPieces,
      metConditions,
      chooseCards
    };
  }
}