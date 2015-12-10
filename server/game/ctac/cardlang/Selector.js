import loglevel from 'loglevel-decorator';
import _ from 'lodash';

@loglevel
export default class Selector{
  constructor(players, pieceState){
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

  //single target selectors
  selectPiece(controllingPlayerId, selector){
    switch(selector){
      case 'RANDOM_CHARACTER':
        return _.sample(this.selectPieces(controllingPlayerId, 'CHARACTERS'));
        break;
      case 'RANDOM_FRIENDLY_CHARACTER':
        return _.sample(this.selectPieces(controllingPlayerId, 'FRIENDLY_CHARACTERS'));
        break;
      case 'RANDOM_ENEMY_CHARACTER':
        return _.sample(this.selectPieces(controllingPlayerId, 'ENEMY_CHARACTERS'));
        break;
      case 'RANDOM_ENEMY_MINION':
        return _.sample(this.selectPieces(controllingPlayerId, 'ENEMY_MINIONS'));
        break;
      case 'RANDOM_FRIENDLY_MINION':
        return _.sample(this.selectPieces(controllingPlayerId, 'FRIENDLY_MINIONS'));
        break;
    }
  }

  //multi target
  selectPieces(controllingPlayerId, selector){
    switch(selector){
      case 'CHARACTERS':
        return new PieceSelector(this.pieceState.pieces)
          .Value();
        break;
      case 'FRIENDLY_CHARACTERS':
        return new PieceSelector(this.pieceState.pieces)
          .Friendlies(controllingPlayerId)
          .Value();
        break;
      case 'ENEMY_CHARACTERS':
        return new PieceSelector(this.pieceState.pieces)
          .Enemies(controllingPlayerId)
          .Value();
        break;
      case 'ENEMY_MINIONS':
        return new PieceSelector(this.pieceState.pieces)
          .Enemies(controllingPlayerId)
          .Minion()
          .Value();
        break;
      case 'FRIENDLY_MINIONS':
        return new PieceSelector(this.pieceState.pieces)
          .Friendlies(controllingPlayerId)
          .Minion()
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

  Friendlies(controllingPlayerId){
    this.pieces = this.pieces.filter(p => p.playerId == controllingPlayerId);
    return this;
  }

  Minion(){
    this.pieces = this.pieces.filter(p => p.tags.indexOf('Minion') >= 0);
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