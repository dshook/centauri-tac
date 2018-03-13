export default class Position
{
 //source: unity scene

 // z       y       x
 //  \      |      /
 //    \    |    /
 //      \  |  /
 //        \|/

  constructor(x, y, z)
  {
    this.x = x;
    this.y = y;
    this.z = z;
  }

  toString() {
      return `(${this.x}, ${this.y}, ${this.z})`;
  }

  equals(otherP){
    return  this.x === otherP.x
      && this.y === otherP.y
      && this.z === otherP.z;
  }

  //ignore height here
  tileEquals(otherP){
    return this.x === otherP.x
      && this.z === otherP.z;
  }

  add(otherP){
    return new Position(this.x + otherP.x, this.y + otherP.y, this.z + otherP.z);
  }

  sub(otherP){
    return new Position(this.x - otherP.x, this.y - otherP.y, this.z - otherP.z);
  }

  addXYZ(x, y, z){
    return new Position(this.x + x, this.y + y, this.z + z);
  }

  addX(amt){
    return new Position(this.x + amt, this.y, this.z);
  }
  addY(amt){
    return new Position(this.x, this.y + amt, this.z);
  }
  addZ(amt){
    return new Position(this.x, this.y, this.z + amt);
  }
}