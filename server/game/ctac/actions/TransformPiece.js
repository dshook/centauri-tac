import BaseAction from './BaseAction.js';

/**
 * I'm CHAANNNGGINGG!
 */
export default class TransformPiece extends BaseAction
{
  constructor({pieceId, cardTemplateId, transformPieceId})
  {
    super();
    this.pieceId = pieceId;
    this.cardTemplateId = cardTemplateId || null;
    this.transformPieceId = transformPieceId || null;

    this.updatedPiece = null;
  }
}
