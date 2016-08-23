export default class CardAura
{
  constructor(pieceId, cardSelector, name)
  {
    this.serverOnly = true;

    this.id = null;
    this.pieceId = pieceId;
    this.cardSelector = cardSelector;
    this.name = name;

    //changes in stats, not abs amount
    this.cost = null;
  }
}
