(function() {
  createStars();
})();

function createStars(){
  createStarSize(700, 'stars1');
  createStarSize(200, 'stars2');
  createStarSize(100, 'stars3');
}

function createStarSize(number, size){
  var starsHolder = document.querySelector('.stars-holder');

  for(var i = 0; i < number; i++){
    var div = document.createElement("div");
    div.style.left = randomInt(0, 2000) + "px";
    div.style.top  = randomInt(0, 2000) + "px";
    div.setAttribute('class', 'star ' + size)
    starsHolder.appendChild(div);
  }
}

function randomInt(min,max)
{
    return Math.floor(Math.random()*(max-min+1)+min);
}