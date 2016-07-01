var fs = require('fs');
var outputFile = './maps/cubeland.json';

var mapXSize = 8;
var mapZSize = 8;
var tileSet = ['clay', 'sand'];

function random(min, max) {
  if (max == null) {
    max = min;
    min = 0;
  }
  return min + Math.floor(Math.random() * (max - min + 1));
}
// random real number in range {min, max}, including min but excluding max
function randomReal(xmin,xmax) {
  return +(Math.random() * (xmax - xmin) + xmin).toFixed(3);
}

function sample(array){
  return array[random(array.length - 1)];
}

var tiles = [];

for(var x = 0; x < mapXSize; x++){
  for(var z = 0; z < mapZSize; z++){
    tiles.push({
      transform: {x, y: randomReal(0, 0.9), z},
      material: sample(tileSet)
    });
  }
}

var map = {
  name: 'cubeland',
  maxPlayers: 2,
  tiles
};

fs.writeFile(outputFile, JSON.stringify(map, null, 4), function(err) {
  if(err) {
    console.log(err);
  } else {
    console.log("JSON saved to " + outputFile);
  }
});