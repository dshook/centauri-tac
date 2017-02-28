import _ from 'lodash';

import Enum from 'enum';

var Races = Enum({
  Neutral: 0,
  Vesudari: 1,
  Earthlings: 2,
  Martians: 3,
  Grex: 4,
  Aensians: 5,
  Lost: 6,
});
export default Races;

export function fromInt(value){
  return (_.invert(Races))[value];
}
