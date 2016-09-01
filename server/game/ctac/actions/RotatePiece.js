import BaseAction from './BaseAction.js';
import {fromInt} from '../models/Direction.js';

export default class RotatePiece extends BaseAction
{
  constructor(pieceId, direction)
  {
    super();
    this.pieceId = pieceId;
    this.direction = fromInt(direction);
  }
}
