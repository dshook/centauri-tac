import _ from 'lodash';

//gross
import Enum from '../../../../lib/enum';

var Statuses = Enum({
  Silence: 1,
  Sheild : 2,
  Paralyze: 3,
  Taunt : 4,
  Cloak: 5,
  TechResist: 6,
  Rooted: 7
});
export default Statuses;

export function fromInt(value){
  return (_.invert(Statuses))[value];
}
