export default class MapModel
{
  constructor(name, maxPlayers)
  {
    this.name = name;
    this.maxPlayers = maxPlayers;
    this.startingPositions = []; //position arrays of hero spawn
    this.tiles = []; //flat array of all tiles
    this.tileMatrix = {}; //tiles stored by x and z positions
  }
}