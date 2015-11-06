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

  incriment(playerId, turnId){
    //check init
    if(this.resources[playerId] === undefined){
      this.resources[playerId] = 0;    
    }
    this.resources[playerId] = Math.floor(turnId / 2) + 1;
    this.resources[playerId] = Math.min(this.resources[playerId], 10);
    return this.resources[playerId];
  }

  expend(playerId, amount){
    this.resources[playerId] -= amount;
    this.resources[playerId] = Math.max(this.resources[playerId], 0);
    return this.resources[playerId];
  }

  get(playerId){
    return this.resources[playerId];
  }
}
