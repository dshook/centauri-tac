import lang from './cardlang.js';

export default function parse(input){
  let parsed = lang.parser.parse(input);
  collapseLayer(parsed);
  return parsed;
}

function isObject(input){
  return input instanceof Object;
}

/*
  //Collapses unnecessary layers like

  left : {
    left: {
      'MINION'
    }
  }

  into:

  left: {
    'MINION'
  }
*/
function collapseLayer(obj){
  if(Array.isArray(obj)){
    for (const item of obj) {
      collapseLayer(item);
    }
  }else{
    if(!isObject(obj)){ return; } //done for primatives

    //simplify left if possible
    if(isObject(obj.left) && Object.keys(obj.left).length === 1 && obj.left.left){
      obj.left = obj.left.left;
    }
    //same for right
    if(isObject(obj.right) && Object.keys(obj.right).length === 1 && obj.right.left ){
      obj.right = obj.right.left;
    }

    //iterate remaining keys
    for (const key in obj) {
      collapseLayer(obj[key]);
    }
  }
}