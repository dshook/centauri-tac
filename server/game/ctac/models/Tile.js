
export default class Tile
{

  constructor(position, unpassable, clearable)
  {
    this.position = position;
    this.unpassable = unpassable || false;
    this.clearable = clearable || false;
  }

}