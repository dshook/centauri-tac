import _ from 'lodash';
import Enum from 'enum';

var Rarities = Enum({
  Free: 0,
  Common: 1,
  Rare: 2,
  Exotic: 3,
  Ascendant: 4,
});
export default Rarities;

export function fromInt(value){
  return (_.invert(Rarities))[value];
}
