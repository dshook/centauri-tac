import {fromInt} from '../models/Direction.js';

export default class RotatePiece
{
  constructor(pieceId, direction)
  {
    this.id = null;
    this.pieceId = pieceId;
    this.direction = fromInt(direction);
  }
}
