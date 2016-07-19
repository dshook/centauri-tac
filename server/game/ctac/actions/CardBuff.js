//Whenever a card is buffed, or had a buff removed
export default class CardBuff
{
  constructor(cardId, name, removed = false, auraPieceId = null)
  {
    this.id = null;
    this.cardId = cardId;
    this.name = name;
    this.removed = removed;
    this.auraPieceId = auraPieceId;

    //changes in stats, not abs amount
    this.cost = null;

    //new values updated by proccessor
    this.newCost = null;
  }
}
