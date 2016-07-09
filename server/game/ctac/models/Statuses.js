import _ from 'lodash';

import Enum from 'enum';

var Statuses = Enum({
  Silence   : 1,
  Shield    : 2,
  Paralyze  : 4,
  Taunt     : 8,
  Cloak     : 16,
  TechResist: 32,
  Root      : 64,
  Charge    : 128,
  CantAttack: 256
});
export default Statuses;

export function fromInt(value){
  return (_.invert(Statuses))[value];
}
