import BaseAction from './BaseAction.js';

export default class AttachCode extends BaseAction
{
  constructor(pieceId, eventList)
  {
    super();
    this.pieceId = pieceId;

    this.eventList = eventList;
  }
}
