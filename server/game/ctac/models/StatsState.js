import loglevel from 'loglevel-decorator';

/**
 * Game stats state, some of which is accessible to cards
 */
@loglevel
export default class StatsState
{
  constructor()
  {
    this.stats = {
      players: {},
    };
  }

  //stores a stat, and if playerId is supplied, store it per player
  setStat(path, value, playerId){
    if(playerId){
      if(!this.stats.players[playerId]){
        this.stats.players[playerId] = {};
      }
      this.stats.players[playerId][path] = value;
    }else{
      this.stats[path] = value;
    }
  }

  //if playerId is specified, first look there, but then fall back to global stats
  getStat(path, playerId){
    if(playerId){
      if(this.stats.players[playerId] && path in this.stats.players[playerId]){
        return this.stats.players[playerId][path];
      }
    }
    return this.stats[path];
  }
}
