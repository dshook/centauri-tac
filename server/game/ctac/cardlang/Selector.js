import loglevel from 'loglevel-decorator';

@loglevel
export default class Selector{
  constructor(turnState, players){
    this.turnState = turnState;
    this.players = players;
  }

  selectPlayer(selector){
    switch(selector){
      case 'PLAYER':
        return this.turnState.currentPlayerId;
        break;
      case 'OPPONENT':
        let opponents = this.players.filter(x => x.id !== this.turnState.currentPlayerId);
        if(opponents.length > 0) 
          return opponents[0].id;
        break;
    }
    this.log.info('Invalid player selector %s', selector);
    return null;
  }
}