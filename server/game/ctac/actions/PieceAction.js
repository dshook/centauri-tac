import BaseAction from './BaseAction.js';

//This action takes a selector, the selector params, and the action and args it will expand into
export default class PieceAction extends BaseAction
{
  constructor(selector, pieceSelectorParams, actionClass, actionParams)
  {
    super();
    this.serverOnly = true;

    this.selector = selector;
    this.pieceSelectorParams = pieceSelectorParams;
    this.actionClass = actionClass;
    this.actionParams = actionParams;

  }
}
