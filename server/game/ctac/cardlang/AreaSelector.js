import loglevel from 'loglevel-decorator';
import EvalError from './EvalError.js';
import Direction from '../models/Direction';
import {adjacentPosition} from '../models/Direction';

 @loglevel
export default class AreaSelector{
  constructor(selector, mapState){
    this.selector = selector;
    this.mapState = mapState;
  }

  //returns an object that defines an area
  //including the type, size, center, if it's based on the cursor or not
  //and a defined area (array of positions) if it's static

  //ex selecting pieces with areas:
  //Area(Square, size, centerSelector)
  //Area(Line, size, centerSelector, pivotSelector, isBothDirections)
  //
  //ex selecting tiles to choose from
  //CURSOR & Area(...)
  //
  //Cross|Square|Diamond should be self explanitory,
  //Line can go any direction diagonal or not
  //Diagonal is limited to diagonal, not horizontal or vertical
  //Row is the opposite of diagonal
  Select(selector, pieceSelectorParams){
    let areaType = null;
    let size = null;
    let centerSelector = null;
    let pivotSelector = null;
    let bothDirections = null;
    let isCursor = false;
    let isDoubleCursor = false;
    let selfCentered = false;
    let centerPosition = null;
    let pivotPosition = null;
    let resolvedPosition = null;
    let stationaryArea = false;
    let areaTiles = [];

    //check to see if this is a choose a tile in an area selection
    if(selector.left === 'CURSOR' && selector.right && selector.right.area){
      isCursor = true;
      selector = selector.right;
      //indicates that the area is fixed on the center position and not attached to the cursor
      //and that the cursor is to select a space within the area
      stationaryArea = true;

      //use pivot position as the 'resolved' position that is the point in the area chosen
      if(pieceSelectorParams.pivotPosition){
        resolvedPosition = pieceSelectorParams.pivotPosition;
      }
    }

    if(selector.area){
      areaType = selector.args[0];
      size = selector.args[1];
      centerSelector = selector.args[2];
      let extraParams = areaType === 'Line' || areaType === 'Diagonal' || areaType === 'Row';
      pivotSelector = extraParams ? selector.args[3] : null;
      bothDirections = extraParams ? selector.args[4] : null;

      // this.log.info('Evaluating area %s size %s %s %s'
      //   , areaType, size
      //   , pieceSelectorParams.position ? 'center ' + pieceSelectorParams.position : '',
      //   pieceSelectorParams.pivotPosition ? 'pivot ' + pieceSelectorParams.pivotPosition : '');

      if(!centerSelector){
        throw 'Center selector missing ' + pieceSelectorParams.activatingPiece;
      }

      //first find the centering piece then all the pieces in the area
      if(centerSelector.left && centerSelector.left === 'CURSOR'){
        isCursor = true;
        if(pieceSelectorParams.position){
          centerPosition = pieceSelectorParams.position;
        }
      }else if(this.selector.doesSelectorUse(centerSelector, 'TARGET') && !pieceSelectorParams.targetPieceId){
        //can't select a ceter piece on a target without a target id
        centerPosition = null;
      }else{
        let centerPieces = this.selector.selectPieces(centerSelector, pieceSelectorParams);
        let centerPiece = centerPieces[0];
        if(centerPiece){
          centerPosition = centerPiece.position;
        }
      }

      //special case to tell the client that the center is around the piece
      if(centerSelector.left && centerSelector.left === 'SELF'){
        selfCentered = true;
      }

      //check for double cursor (like to select two points for a line)
      if(pivotSelector && pivotSelector.left && pivotSelector.left === 'CURSOR'){
        if(isCursor){
          isDoubleCursor = true;
        }
        isCursor = true;
        if(pieceSelectorParams.pivotPosition){
          pivotPosition = pieceSelectorParams.pivotPosition;
        }
      }else if(pivotSelector && pivotSelector.areaDirection){
        //select relative directions to the center position
        let selectedDirection = Direction[pivotSelector.areaDirection];
        pivotPosition = adjacentPosition(centerPosition, selectedDirection, areaType === 'Diagonal')
      }else if(pivotSelector){
        let pivotPieces = this.selector.selectPieces(pivotSelector, pieceSelectorParams);
        let pivotPiece = pivotPieces[0];
        if(pivotPiece){
          pivotPosition = pivotPiece.position;
        }
      }

      //this.log.info('Selected center of %s and pivot of %s', centerPosition, pivotPosition);

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
          case 'Diamond':
            areaTiles = this.mapState.getTilesInRadius(centerPosition, size);
            break;
          case 'Line': {
            //check to see that the pivot position is close to the center and in one of the cross tiles
            let kingNeighbors = this.mapState.getKingTilesInRadius(centerPosition, 1);
            let foundNeighbor = kingNeighbors.find(p => p.tileEquals(pivotPosition));
            if(!foundNeighbor){
              this.log.error('Invalid neighbor for line. Center ' + centerPosition + ' pivot ' + pivotPosition);
              throw new EvalError('Invalid neighbor for line.');
            }
            areaTiles = this.mapState.getLineTiles(centerPosition, pivotPosition, size, bothDirections);
            break;
          }
          case 'Row': {
            //check to see that the pivot position is close to the center and in one of the cross tiles
            let neighbors = this.mapState.getNeighbors(centerPosition);
            let foundNeighbor = neighbors.find(p => p.tileEquals(pivotPosition));
            if(!foundNeighbor){
              this.log.error('Invalid neighbor for row. Center ' + centerPosition + ' pivot ' + pivotPosition);
              throw new EvalError('Invalid neighbor for row.');
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
              this.log.error('Invalid neighbor for diagonal. Center ' + centerPosition + ' pivot ' + pivotPosition)
              throw new EvalError('Invalid neighbor for diagonal.');
            }
            areaTiles = this.mapState.getLineTiles(centerPosition, pivotPosition, size, bothDirections);
            break;
          default:
            throw 'Invalid Area selection type ' + areaType;
        }
      }

      //verify selected point in area is actually within the area
      if(resolvedPosition){
        if(!areaTiles.find(p => p.tileEquals(resolvedPosition))){
          throw 'Selected point is not within area';
        }
      }
    }else if(selector.left){
      this.log.info('Evaluating area for normal piece selector %j', selector);
      //area position from normal piece selector.  Use first found piece
      let selectedPieces = this.selector.selectPieces(selector, pieceSelectorParams);
      if(selectedPieces.length > 0){
        let firstPiece = selectedPieces[0];
        resolvedPosition = firstPiece.position;
        areaType = 'PiecePosition';
        size = 1;
      }
    }

    return {
      areaType,
      size,
      isCursor,
      isDoubleCursor,
      bothDirections,
      selfCentered,
      stationaryArea,
      centerPosition,
      pivotPosition,
      areaTiles,
      resolvedPosition
    };
  }

}
