import SpawnDeck from '../actions/SpawnDeck.js';
import Card from '../models/Card.js';
import loglevel from 'loglevel-decorator';
import _ from 'lodash';

/**
 * Handle the PassTurn action
 */
@loglevel
export default class SpawnDeckProcessor
{
  constructor(cardDirectory, cardState)
  {
    this.cardDirectory = cardDirectory;
    this.cardState = cardState;
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
    let cardIds = _.map(playableCards, (m) => m.cardTemplateId);

    let playerId = action.playerId;
    let deck = this.cardState.decks[playerId];

    //dev hack, set one card you're working on to be most of your deck
    let testingCards = [49, 2, 40];

    for(let c = 0; c < deckCards; c++){
      let randCardId = _.sample(cardIds);
      if(c % 3 == 0) randCardId = _.sample(testingCards);

      //add the credit at the right spot
      if(action.startingPlayerId != playerId && action.initialDrawAmount - 1 === c){
        let credit = this.cardDirectory.newFromId(44);
        this.cardState.addToDeck(playerId, credit);
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
