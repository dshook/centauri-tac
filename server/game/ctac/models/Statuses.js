import _ from 'lodash';

import Enum from 'enum';

var Statuses = Enum({
  None       : 0,
  Silence    : 1,
  Shield     : 2,
  Petrify    : 4,
  Taunt      : 8,
  Cloak      : 16,
  Elusive    : 32,
  Root       : 64,
  Charge     : 128,
  CantAttack : 256,
  DyadStrike : 512,
  Flying     : 1024,
  Airdrop    : 2048,
  Cleave     : 4096,
  Piercing   : 8192,
});
export default Statuses;

export function fromInt(value){
  return (_.invert(Statuses))[value];
}
