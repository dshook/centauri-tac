import loglevel from 'loglevel-decorator';
import _ from 'lodash';

@loglevel
export default class Selector{
  constructor(turnState, players, pieceState){
    this.turnState = turnState;
    this.players = players;
    this.pieceState = pieceState;
  }

  selectPlayer(controllingPlayerId, selector){
    switch(selector){
      case 'PLAYER':
        return controllingPlayerId;
        break;
      case 'OPPONENT':
        let opponents = this.players.filter(x => x.id !== controllingPlayerId);
        if(opponents.length > 0) 
          return opponents[0].id;
        break;
    }
    this.log.info('Invalid player selector %s', selector);
    return null;
  }

  selectPiece(controllingPlayerId, selector){
    switch(selector){
      case 'RANDOM_CHARACTER':
        return new PieceSelector(this.pieceState.pieces)
          .Random()
          .Value();
        break;
      case 'RANDOM_ENEMY_CHARACTER':
        return new PieceSelector(this.pieceState.pieces)
          .Enemies(controllingPlayerId)
          .Random()
          .Value();
        break;
    }
  }

}

class PieceSelector{
  constructor(pieces){
    this.pieces = _.chain(pieces);
  }

  Enemies(controllingPlayerId){
    this.pieces = this.pieces.filter(p => p.playerId != controllingPlayerId);
    return this;
  }

  Random(){
    this.pieces = this.pieces.sample();
    return this;
  }

  Value(){
    return this.pieces.value();
  }

}