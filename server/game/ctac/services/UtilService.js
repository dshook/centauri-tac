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
  findPossibleActions(playerId){
    //look through the current players hand for any cards needing a TARGET
    //and through pieces for possible abilities
    let targets = this.cardEvaluator.findPossibleTargets(
      this.cardState.hands[playerId],
      playerId
    );
    let abilities = this.cardEvaluator.findPossibleAbilities(
      this.pieceState.pieces,
      playerId
    );
    let areas = this.cardEvaluator.findPossibleAreas(
      this.cardState.hands[playerId],
      playerId
    );
    let metConditions = this.cardEvaluator.findMetConditionCards(
      this.cardState.hands[playerId],
      playerId
    );
    let eventedPieces = this.cardEvaluator.findEventedPieces();
    let chooseCards = this.cardEvaluator.findChooseCards(
      this.cardState.hands[playerId],
      playerId,
      this.cardDirectory
    );
    return {
      playerId: playerId,
      spellDamage: this.pieceState.totalSpellDamage(playerId),
      targets,
      areas,
      abilities,
      eventedPieces,
      metConditions,
      chooseCards
    };
  }
}