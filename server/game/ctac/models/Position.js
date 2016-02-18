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
    return this.x === otherP.x
        && this.y === otherP.y
        && this.z === otherP.z;
  }

}