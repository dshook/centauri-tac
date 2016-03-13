import _ from 'lodash';

//gross
import Enum from '../../../../lib/enum';

var Direction = Enum({
  North: 1,
  East : 2,
  South: 3,
  West : 4
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
      throw 'Invalid Directions to face';
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