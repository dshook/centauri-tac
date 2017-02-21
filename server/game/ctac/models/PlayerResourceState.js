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
    this.maxResources = {};
    this.charges = {};
    this.resourceCap = 10;
  }

  init(playerId){
    if(this.resources[playerId] === undefined){
      this.resources[playerId] = 0;
      this.maxResources[playerId] = 0;
      this.charges[playerId] = 0;
    }
  }

  incriment(playerId, amount){
    this.maxResources[playerId] += amount;
    this.maxResources[playerId] = Math.min(this.maxResources[playerId], this.resourceCap);
    return this.maxResources[playerId];
  }

  refill(playerId){
    this.resources[playerId] = this.maxResources[playerId];
    return this.resources[playerId];
  }

  reset(playerId){
    this.resources[playerId] = 0;
    return this.resources[playerId];
  }

  adjust(playerId, amount){
    this.resources[playerId] += amount;
    //limit the change to zero and the max.  Don't cap at maxResources so you can
    //temporarily go over
    this.resources[playerId] = Math.max(this.resources[playerId], 0);
    this.resources[playerId] = Math.min(this.resources[playerId], this.resourceCap);
    return this.resources[playerId];
  }

  get(playerId){
    return this.resources[playerId];
  }
  getMax(playerId){
    return this.maxResources[playerId];
  }

  getByPath(resourceKey, playerId){
    let fixedKey = resourceKey.charAt(0).toLowerCase() + resourceKey.slice(1);
    if(!this[fixedKey]) return null;
    return this[fixedKey][playerId];
  }
}
