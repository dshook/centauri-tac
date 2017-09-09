//Error for saving decks
export default class DeckStoreError extends Error
{
  constructor(message)
  {
    super();
    this.message = message;
    this.name = 'DeckStoreError';
  }
}
