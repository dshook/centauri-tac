import SpawnDeck from '../actions/SpawnDeck.js';
import loglevel from 'loglevel-decorator';
import _ from 'lodash';
import Constants from '../util/Constants.js';

@loglevel
export default class SpawnDeckProcessor
{
  constructor(cardDirectory, cardState, gameConfig, deckInfo)
  {
    this.cardDirectory = cardDirectory;
    this.cardState = cardState;
    this.gameConfig = gameConfig;
    this.deckInfo = deckInfo;
  }

  /**
   * give players some cards and init decks and hands
   */
  async handleAction(action, queue)
  {
    if (!(action instanceof SpawnDeck)) {
      return;
    }

    let playerDeckInfo = this.deckInfo.find(d => d.playerId === action.playerId);
    if(!playerDeckInfo){
      this.log.error('Cannot find player deck info to spawn deck for player %s', action.playerId);
      return queue.cancel(action);
    }

    let playerId = action.playerId;
    let deck = this.cardState.decks[playerId];

    //all legit cases should have cards in the deck
    if(playerDeckInfo.cards && playerDeckInfo.cards.length){
      for(let c = 0; c < playerDeckInfo.cards.length; c++){
        let cardClone = this.cardDirectory.newFromId(playerDeckInfo.cards[c].cardTemplateId);

        this.cardState.addToDeck(playerId, cardClone);
      }
      this.cardState.shuffleDeck(playerId);

    }else{
      //but for testing, spawn a random deck
      this.log.info('Spawning random deck for player %s', action.playerId);

      let deckCards = Constants.deckCardLimit;
      var playableCards = this.cardDirectory.getByTag(['Minion', 'Spell']);
      let cardIds = playableCards
        .filter(c => !c.uncollectible && c.race === 0 || c.race === action.race)
        .map(m => m.cardTemplateId);

      for(let c = 0; c < deckCards; c++){
        let randCardId = _.sample(cardIds);
        if(this.gameConfig.testingCards.length && c % 2 == 0){
          randCardId = _.sample(this.gameConfig.testingCards);
        }

        let cardClone = this.cardDirectory.newFromId(randCardId);

        this.cardState.addToDeck(playerId, cardClone);
      }
    }

    action.cards = deck.length;

    queue.complete(action);
    this.log.info('deck of %s cards for player %s',
      deck.length, action.playerId);
  }
}
