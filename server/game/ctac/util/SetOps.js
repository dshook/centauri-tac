import _ from 'lodash';

export function Union(a, b, equal){
  var results = [];
  results = results.concat(a);

  for(let bElement of b){
    if(!_.some(results, res => equal(bElement, res) )) {
        results.push(bElement);
    }
  }

  return results;
}

export function Intersection(a, b, equal){
  var results = [];

  for(var i = 0; i < a.length; i++) {
      var aElement = a[i];

      if(_.some(b, bElement => equal(bElement, aElement) )) {
          results.push(aElement);
      }
  }

  return results;
}

// a - b
export function Difference(a, b, equal){
  var results = [];

  for(var i = 0; i < a.length; i++) {
      var aElement = a[i];

      if(!_.some(b, bElement => equal(bElement, aElement) )) {
          results.push(aElement);
      }
  }

  return results;
}
