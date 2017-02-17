import _ from 'lodash';

import Enum from 'enum';

var Rarities = Enum({
  Common: 0,
  Rare: 1,
  Exotic: 2,
  Mythical: 3,
});
export default Rarities;

export function fromInt(value){
  return (_.invert(Rarities))[value];
}
