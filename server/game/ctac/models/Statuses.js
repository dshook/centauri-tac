import _ from 'lodash';

//gross
import Enum from '../../../../lib/enum';

var Statuses = Enum({
  Silence   : 1,
  Shield    : 2,
  Paralyze  : 4,
  Taunt     : 8,
  Cloak     : 16,
  TechResist: 32,
  Root      : 64
});
export default Statuses;

export function fromInt(value){
  return (_.invert(Statuses))[value];
}
