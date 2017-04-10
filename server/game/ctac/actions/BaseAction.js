export default class BaseAction
{
  constructor()
  {
    this.id = null;
    this.activatingPieceId = null;

    //server only props which get deleted before sent to client
    this.pieceSelectorParams = null;
    this.selector = null;
  }
}
