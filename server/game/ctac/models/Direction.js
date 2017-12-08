import _ from 'lodash';

import Enum from 'enum';

var Direction = Enum({
  North: 1, //plus z
  East : 2, //plus x
  South: 3, //minus z
  West : 4  //minus x
});
export default Direction;

export function fromInt(value){
  return (_.invert(Direction))[value];
}

export function directionOf(dir1, dir2){
  var diff = dir2 - dir1;
  if(diff < 0) diff += 4;

  switch(diff){
    case 0:
      return 'behind';
      break;
    case 1:
    case 3:
      return 'side';
      break;
    case 2:
      return 'facing';
      break;
    default:
      throw 'Invalid Direction to face dir1 ' + dir1 + ' dir2 ' + dir2;
  }
}

//given a start and finishing position return the direction you should turn
export function faceDirection(position1, position2){
  let difference = position1.sub(position2);
  let targetDirection = Direction.South;
  if (difference.x > 0)
  {
    targetDirection = Direction.East;
  }
  else if (difference.x < 0)
  {
    targetDirection = Direction.West;
  }
  else if (difference.z > 0)
  {
    targetDirection = Direction.North;
  }
  else if (difference.z < 0)
  {
    targetDirection = Direction.South;
  }
  return targetDirection;
}

//Gets the adjacent tiles for cleave given an attacker position and the direction they're facing
export function cleavePositions(position, direction){
  switch(direction)
  {
    case 1:
      return [position.addXYZ(1,0,1), position.addXYZ(-1,0,1)];
    case 2:
      return [position.addXYZ(1,0,-1), position.addXYZ(1,0,1)];
    case 3:
      return [position.addXYZ(1,0,-1), position.addXYZ(-1,0,-1)];
    case 4:
      return [position.addXYZ(-1,0,-1), position.addXYZ(-1,0,1)];
  }
}

export function piercePositions(position, direction){
  switch(direction)
  {
    case 1:
      return [position.addZ(2), position.addZ(3)];
    case 2:
      return [position.addX(2), position.addX(3)];
    case 3:
      return [position.addZ(-2), position.addZ(-3)];
    case 4:
      return [position.addX(-2), position.addX(-3)];
  }
}
