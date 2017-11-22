(function() {
  createStars();
})();

function createStars(){
  createStarSize(500, 'stars1');
  createStarSize(150, 'stars2');
  createStarSize(100, 'stars3');
}

function createStarSize(number, size){
  var starsHolder = document.querySelector('.stars-holder');

  for(var i = 0; i < number; i++){
    var div = document.createElement("div");
    div.style.left = (randomInt(0, 10000) / 100) + "%";
    div.style.top  = (randomInt(0, 10000) / 100) + "%";
    div.setAttribute('class', 'star ' + size)
    starsHolder.appendChild(div);
  }
}

function randomInt(min,max)
{
    return Math.floor(Math.random()*(max-min+1)+min);
}