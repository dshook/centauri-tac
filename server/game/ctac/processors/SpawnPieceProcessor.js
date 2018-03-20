import _ from 'lodash';
import SpawnPiece from '../actions/SpawnPiece.js';
import SetPlayerResource from '../actions/SetPlayerResource.js';
import loglevel from 'loglevel-decorator';

@loglevel
export default class SpawnPieceProcessor
{
  constructor(pieceState, players, cardDirectory, cardEvaluator, cardState, turnState, statsState, mapState)
  {
    this.pieceState = pieceState;
    this.players = players;
    this.cardDirectory = cardDirectory;
    this.cardEvaluator = cardEvaluator;
    this.cardState = cardState;
    this.turnState = turnState;
    this.statsState = statsState;
    this.mapState = mapState;
  }

  /**
   * Proc
   */
  async handleAction(action, queue)
  {
    if (!(action instanceof SpawnPiece)) {
      return;
    }

    //resolve final position if it's a spawn in radius
    if(action.spawnKingRadius){
      let possiblePositions = this.mapState.getKingTilesInRadius(action.position, action.spawnKingRadius);
      possiblePositions = possiblePositions.filter(p =>
        !this.pieceState.pieceAt(p) && !this.mapState.getTile(p).unpassable
      );
      if(possiblePositions.length > 0){
        action.position = _.sample(possiblePositions);
      }else{
        this.log.info('Couldn\'t spawn piece because there\'s no where to put it');
        return queue.cancel(action, false);
      }
    }

    let cardPlayed = this.cardDirectory.directory[action.cardTemplateId];

    if(!cardPlayed){
      this.log.warn('Cannot find cardTemplateId %s in directory to spawn', action.cardTemplateId);
      return queue.cancel(action, true);
    }

    //check if played in an unoccupied spot
    let occupyingPiece = this.pieceState.pieceAt(action.position);
    if(occupyingPiece){
      //be sure to emit the cancel event so the client can respond
      this.log.warn('Can\'t spawn piece %s at position %s because %j is occupying it',
        cardPlayed.name, action.position, occupyingPiece);
      return queue.cancel(action, true);
    }

    let destinationTile = this.mapState.getTile(action.position);
    //and then unpassable tiles
    if(destinationTile.unpassable){
      this.log.warn('Cannot play minion on unpassable tile');
      return queue.cancel(action, true);
    }

    var newPiece = this.pieceState.newFromCard(this.cardDirectory, action.cardTemplateId, action.playerId, action.position);
    newPiece.direction = action.direction;

    //For pieces that spawn from cards and don't have an activating piece Id set already, set it to itself
    //So the client can group together battlecry actions
    if(!action.activatingPieceId){
      action.activatingPieceId = newPiece.id;
    }

    if(this.cardEvaluator.evaluatePieceEvent('playMinion', newPiece, {
      targetPieceId: action.targetPieceId,
      position: action.position,
      pivotPosition: action.pivotPosition,
      chooseCardTemplateId: action.chooseCardTemplateId
      })
    ){
      action.pieceId = this.pieceState.add(newPiece, this.turnState.currentTurn);
      action.tags = newPiece.tags;

      if(action.cardInstanceId !== null){
        const playedCard = this.cardState.playCard(action.playerId, action.cardInstanceId);
        if(!playedCard){
          this.log.error('Card id %s was not found in player %s\'s hand', action.cardInstanceId, action.playerId);
          return queue.cancel(action, true);
        }

        this.statsState.setStat('COMBOCOUNT', 1 + this.statsState.getStat('COMBOCOUNT', action.playerId), action.playerId);
      }

      queue.complete(action);
      this.log.info('spawned piece %s for player %s at %s',
        action.cardTemplateId, action.playerId, action.position);

      if(action.cardInstanceId !== null){
        queue.push(new SetPlayerResource(action.playerId, -cardPlayed.cost));
      }
    }else{
      //be sure to emit the cancel event so the client can respond
      this.log.info('Spawned piece %s for player %s SCRUBBED',
        action.cardTemplateId, action.playerId, action.position);
      return queue.cancel(action, true);
    }

  }
}
