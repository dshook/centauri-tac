import _ from 'lodash';

import Enum from 'enum';

var Statuses = Enum({
  None       : 0,
  Silence    : 1 << 0,
  Shield     : 1 << 1,
  Paralyze   : 1 << 2,
  Taunt      : 1 << 3,
  Cloak      : 1 << 4,
  TechResist : 1 << 5,
  Root       : 1 << 6,
  Charge     : 1 << 7,
  CantAttack : 1 << 8,
  DyadStrike : 1 << 9,
  Flying     : 1 << 10,
  Airdrop    : 1 << 11,
});
export default Statuses;

export function fromInt(value){
  return (_.invert(Statuses))[value];
}
