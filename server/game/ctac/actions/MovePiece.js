/**
 * Action that attemps to move a game piece along a route
 */
export default class MovePiece
{
  constructor(pieceId, route)
  {
    this.pieceId = pieceId;
    this.route = route;
  }
}
