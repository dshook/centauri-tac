import SpawnDeck from '../actions/SpawnDeck.js';
import loglevel from 'loglevel-decorator';
import _ from 'lodash';

/**
 * Handle the PassTurn action
 */
@loglevel
export default class SpawnDeckProcessor
{
  constructor(cardDirectory, decks)
  {
    this.cardDirectory = cardDirectory;
    this.decks = decks;
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
    var minions = this.cardDirectory.getByTag('Minion');
    let cardIds = _.map(minions, (m) => m.id);

    let playerId = action.playerId;
    let deck = this.decks[playerId];

    //dev hack, set one card you're working on to be half your deck
    let testingCards = [15, 16];

    for(let c = 0; c < deckCards; c++){
      let randCardId = _.sample(cardIds);
      if(c % 4 == 0) randCardId = _.sample(testingCards);
      deck.push( this.cardDirectory.directory[randCardId]);
    }

    action.cards = deck.length;

    queue.complete(action);
    this.log.info('deck of %s cards for player %s',
      deck.length, action.playerId);
  }
}
