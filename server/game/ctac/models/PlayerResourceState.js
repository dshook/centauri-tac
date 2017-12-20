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
    this.neededResources = {}; //how much energy each player will get this turn, should be max - any carryover
    this.resourceCap = 10;

    this.charges = {};
  }

  init(playerId){
    if(this.resources[playerId] === undefined){
      this.resources[playerId] = 0;
      this.maxResources[playerId] = 0;
      this.neededResources[playerId] = 0;
      this.charges[playerId] = 0;
    }
  }

  incrimentForTurn(playerId, amount){
    this.maxResources[playerId] += amount;
    this.maxResources[playerId] = Math.min(this.maxResources[playerId], this.resourceCap);
    this.adjust(playerId, 1, true);
    this.neededResources[playerId] = this.getMax(playerId) - this.get(playerId);
    return {
      max: this.maxResources[playerId],
      current: this.resources[playerId]
    };
  }

  refill(playerId){
    this.resources[playerId] = this.maxResources[playerId];
    return this.resources[playerId];
  }

  reset(playerId){
    this.resources[playerId] = 0;
    return this.resources[playerId];
  }

  adjust(playerId, amount, capAtMaxResources = false){
    this.resources[playerId] += amount;
    //limit the change to zero and the max.  Don't cap at maxResources so you can temporarily go over if flag isn't set
    this.resources[playerId] = Math.max(this.resources[playerId], 0);
    this.resources[playerId] = Math.min(this.resources[playerId], this.resourceCap);
    if(capAtMaxResources){
      this.resources[playerId] = Math.min(this.resources[playerId], this.maxResources[playerId]);
    }
    return this.resources[playerId];
  }

  changeNeeded(playerId, amount){
    this.neededResources[playerId] += amount;
  }

  get(playerId){
    return this.resources[playerId];
  }
  getMax(playerId){
    return this.maxResources[playerId];
  }
  getNeeded(playerId){
    return this.neededResources[playerId];
  }

  getByPath(resourceKey, playerId){
    let fixedKey = resourceKey.charAt(0).toLowerCase() + resourceKey.slice(1);
    if(!this[fixedKey]) return null;
    return this[fixedKey][playerId];
  }
}
