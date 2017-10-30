import BaseAction from './BaseAction.js';

/**
 * When tiles with props on them are hit with aoe's and made passable
 */
export default class TilesCleared extends BaseAction
{
  constructor(tilePositions)
  {
    super();
    this.tilePositions = tilePositions;
  }
}
