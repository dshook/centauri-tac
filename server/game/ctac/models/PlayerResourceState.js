import loglevel from 'loglevel-decorator';

const maxResources = 10;
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

    this.resources[playerId] = Math.ceil(turnId / 2);
    this.resources[playerId] = Math.min(this.resources[playerId], maxResources);
    return this.resources[playerId];
  }

  adjust(playerId, amount){
    this.resources[playerId] += amount;
    this.resources[playerId] = Math.max(this.resources[playerId], 0);
    this.resources[playerId] = Math.min(this.resources[playerId], maxResources);
    return this.resources[playerId];
  }

  get(playerId){
    return this.resources[playerId];
  }
}
