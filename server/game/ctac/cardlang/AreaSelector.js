import loglevel from 'loglevel-decorator';

 @loglevel
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
      let areaType = selector.args[0];
      let size = selector.args[1];
      let centerSelector = selector.args[2];
      let extraParams = areaType === 'Line' || areaType === 'Diagonal';
      let pivotSelector = extraParams ? selector.args[3] : null;
      let bothDirections = extraParams ? selector.args[4] : null;

      let isCursor = false;
      let isDoubleCursor = false;
      let selfCentered = false;
      let centerPosition = null;
      let pivotPosition = null;
      this.log.info('Evaluating area %s size %s, center %s pivot %s'
        , areaType, size, pieceSelectorParams.position, pieceSelectorParams.pivotPosition);
      if(centerSelector.left && centerSelector.left === 'CURSOR'){
        isCursor = true;
        if(pieceSelectorParams.position){
          centerPosition = pieceSelectorParams.position;
        }
      }else{
        let centerPieces = this.selector.selectPieces(controllingPlayerId, centerSelector, pieceSelectorParams);
        let centerPiece = centerPieces[0];
        if(centerPiece){
          centerPosition = centerPiece.position;
        }
      }

      //special case to tell the client that the center is around the piece
      if(centerSelector.left && centerSelector.left === 'SELF'){
        selfCentered = true;
      }

      if(pivotSelector && pivotSelector.left && pivotSelector.left === 'CURSOR'){
        if(isCursor){
          isDoubleCursor = true;
        }
        isCursor = true;
        if(pieceSelectorParams.pivotPosition){
          pivotPosition = pieceSelectorParams.pivotPosition;
        }
      }else if(pivotSelector){
        let pivotPieces = this.selector.selectPieces(controllingPlayerId, pivotSelector, pieceSelectorParams);
        let pivotPiece = pivotPieces[0];
        if(pivotPiece){
          pivotPosition = pivotPiece.position;
        }
      }

      this.log.info('Selected center of %s and pivot of %s', centerPosition, pivotPosition);

      let areaTiles = [];

      //if we have a defined center, actually resolve what the area tiles are
      //also check for the required pivot position for lines/diagonals
      if(centerPosition && (!extraParams || pivotPosition)){
        switch(areaType){
          case 'Cross':
            areaTiles = this.mapState.getCrossTiles(centerPosition, size);
            break;
          case 'Square':
            areaTiles = this.mapState.getKingTilesInRadius(centerPosition, size);
            break;
          case 'Line': {
            //check to see that the pivot position is close to the center and in one of the cross tiles
            let neighbors = this.mapState.getNeighbors(centerPosition);
            let foundNeighbor = neighbors.find(p => p.tileEquals(pivotPosition));
            if(!foundNeighbor){
              throw 'Invalid neighbor for line. Center ' + centerPosition + ' pivot ' + pivotPosition;
            }
            areaTiles = this.mapState.getLineTiles(centerPosition, pivotPosition, size, bothDirections);
            break;
          }
          case 'Diagonal':
            //check to see that the pivot position is close to the center and in one of the diagonal tiles
            let kingNeighbors = this.mapState.getKingTilesInRadius(centerPosition, 1);
            let lineNeighbors = this.mapState.getNeighbors(centerPosition);
            let foundKingNeighbor = kingNeighbors.find(p => p.tileEquals(pivotPosition));
            let foundLineNeighbor = lineNeighbors.find(p => p.tileEquals(pivotPosition));
            if(!foundKingNeighbor || foundLineNeighbor){
              throw 'Invalid neighbor for diagonal. Center ' + centerPosition + ' pivot ' + pivotPosition;
            }
            areaTiles = this.mapState.getLineTiles(centerPosition, pivotPosition, size, bothDirections);
            break;
          default:
            throw 'Invalid Area selection type ' + areaType;
        }
      }

      return {
        areaType,
        size,
        isCursor,
        isDoubleCursor,
        bothDirections,
        selfCentered,
        centerPosition,
        pivotPosition,
        areaTiles
      };
    }
  }

}
