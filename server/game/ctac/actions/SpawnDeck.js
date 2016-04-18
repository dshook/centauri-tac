export default class SpawnDeck
{
  constructor(playerId, startingPlayerId, initialDrawAmount)
  {
    this.playerId = playerId;
    this.startingPlayerId = startingPlayerId;
    this.initialDrawAmount = initialDrawAmount;
    this.cards = null;
  }
}
