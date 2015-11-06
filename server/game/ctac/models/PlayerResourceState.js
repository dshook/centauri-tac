import loglevel from 'loglevel-decorator';

/**
 * Current state of players resources, ie mana
 */
@loglevel
export default class PlayerResourceState
{
  constructor()
  {
    //map from playerId -> resource
    this.resources = {};
  }

  incriment(playerId){
    //check init
    if(this.resources[playerId] === undefined){
      this.resources[playerId] = 0;    
    }
    this.resources[playerId]++;
    return this.resources[playerId];
  }

  get(playerId){
    return this.resources[playerId];
  }
}
