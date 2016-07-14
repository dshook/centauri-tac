var fs = require('fs');
var outputFile = './maps/cubeland.json';

var mapXSize = 11;
var mapZSize = 11;
var tileSet = ['grass', 'rock', 'clay'];
var center = {x: Math.floor(mapXSize / 2), z: Math.floor(mapZSize / 2)};

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

function tileDistance(posA, posB){
  return Math.abs(posA.x - posB.x) + Math.abs(posA.z - posB.z);
}

var tiles = [];

for(var x = 0; x < mapXSize; x++){
  for(var z = 0; z < mapZSize; z++){
    var position = {x, z};
    var distFromCenter = tileDistance(position, center);
    position.y = (distFromCenter * 0.1).toFixed(3);
    var material = 'grass';
    if(distFromCenter > 2){
      material = 'clay';
    }
    if(distFromCenter > 5){
      material = 'rock';
    }
    tiles.push({
      transform: position,
      material: material
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