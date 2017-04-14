import BaseAction from './BaseAction.js';

//Add actions that will be performed at some point in the future (after x start/end turns)
export default class ActionTimer extends BaseAction
{
  constructor({isStartTimer, pieceSelectorParams, piece, card, playerId, interval, timer, timerActions})
  {
    super();
    this.serverOnly = true;

    this.isStartTimer = isStartTimer || true;
    this.piece = piece || null;
    this.card = card || null;
    this.playerId = playerId;
    this.interval = interval || false;
    this.timer = timer;
    this.timerActions = timerActions;
    this.pieceSelectorParams = pieceSelectorParams || {};
  }
}
