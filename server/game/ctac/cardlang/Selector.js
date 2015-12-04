import loglevel from 'loglevel-decorator';

@loglevel
export default class Selector{
  constructor(turnState, players){
    this.turnState = turnState;
    this.players = players;
  }

  selectPlayer(piece, selector){
    switch(selector){
      case 'PLAYER':
        return piece.playerId;
        break;
      case 'OPPONENT':
        let opponents = this.players.filter(x => x.id !== piece.playerId);
        if(opponents.length > 0) 
          return opponents[0].id;
        break;
    }
    this.log.info('Invalid player selector %s', selector);
    return null;
  }
}