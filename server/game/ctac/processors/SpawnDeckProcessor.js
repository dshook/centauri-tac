import SpawnDeck from '../actions/SpawnDeck.js';
import loglevel from 'loglevel-decorator';
import _ from 'lodash';

@loglevel
export default class SpawnDeckProcessor
{
  constructor(cardDirectory, cardState, gameConfig)
  {
    this.cardDirectory = cardDirectory;
    this.cardState = cardState;
    this.gameConfig = gameConfig;
  }

  /**
   * Proc
   */
  async handleAction(action, queue)
  {
    if (!(action instanceof SpawnDeck)) {
      return;
    }
    //give players some cards and init decks and hands
    let deckCards = 30;
    var playableCards = this.cardDirectory.getByTag(['Minion', 'Spell']);
    let cardIds = playableCards
      .filter(c => !c.uncollectible && c.race === 0 || c.race === action.race)
      .map(m => m.cardTemplateId);

    let playerId = action.playerId;
    let deck = this.cardState.decks[playerId];

    for(let c = 0; c < deckCards; c++){
      let randCardId = _.sample(cardIds);
      if(this.gameConfig.testingCards.length && c % 2 == 0){
        randCardId = _.sample(this.gameConfig.testingCards);
      }

      let cardClone = this.cardDirectory.newFromId(randCardId);

      this.cardState.addToDeck(playerId, cardClone);
    }

    action.cards = deck.length;

    queue.complete(action);
    this.log.info('deck of %s cards for player %s',
      deck.length, action.playerId);
  }
}
