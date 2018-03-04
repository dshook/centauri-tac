/**
 * Configuration dealing with the component manager etc
 */
export default class GameConfig
{
  constructor()
  {
    this.dev = process.env.DEV || false;

    //dev hack, set one card you're working on to be most of your deck
    this.testingCards = (process.env.TESTING_CARDS || '').split(',').filter(x => x);

    //card sets that are enabled.
    this.cardSets = (process.env.CARD_SETS || '').split(',').filter(x => x);
  }
}
