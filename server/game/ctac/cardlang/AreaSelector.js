export default class AreaSelector{
  constructor(selector, mapState){
    this.selector = selector;
    this.mapState = mapState;
  }

  //returns an object that defines an area
  //including the type, size, center, if it's based on the cursor or not
  //and a defined area (array of positions) if it's static
  Select(selector, controllingPlayerId, pieceSelectorParams){
    //first find the centering piece then all the pieces in the area
    if(selector.area){
      let centerPieceSelector = selector.args[0];
      let areaType = selector.args[1];
      let size = selector.args[2];

      let isCursor = false;
      let centerPosition = null;
      if(centerPieceSelector.left && centerPieceSelector.left === 'CURSOR'){
        isCursor = true;
        if(pieceSelectorParams.position){
          centerPosition = pieceSelectorParams.position;
        }
      }else{
        let centerPieces = this.selector.selectPieces(controllingPlayerId, centerPieceSelector, pieceSelectorParams);
        let centerPiece = centerPieces[0];
        if(centerPiece){
          centerPosition = centerPiece.position;
        }
      }

      let areaTiles = [];

      //if we have a defined center, actually resolve what the area tiles are
      if(centerPosition){
        switch(areaType){
          case 'Cross':
            areaTiles = this.mapState.getCrossTiles(centerPosition, size);
            break;
          case 'Square':
            areaTiles = this.mapState.getKingTilesInRadius(centerPosition, size);
            break;
          case 'Line':
            break;
          case 'Diagonal':
            break;
          default:
            throw 'Invalid Area selection type ' + areaType;
        }
        //always remove piece center tile.
        areaTiles = areaTiles.filter(t => !t.tileEquals(centerPosition));
      }

      return {
        areaType,
        size,
        isCursor,
        areaTiles
      };
    }
  }

}
