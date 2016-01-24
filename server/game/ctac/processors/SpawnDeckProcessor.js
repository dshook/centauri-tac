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
  constructor(cardDirectory, decks)
  {
    this.cardDirectory = cardDirectory;
    this.decks = decks;
    this.id = 1;
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
    let cardIds = _.map(playableCards, (m) => m.cardId);

    let playerId = action.playerId;
    let deck = this.decks[playerId];

    //dev hack, set one card you're working on to be half your deck
    let testingCards = [15];

    for(let c = 0; c < deckCards; c++){
      let randCardId = _.sample(cardIds);
      if(c % 4 == 0) randCardId = _.sample(testingCards);

      let directoryCard = this.cardDirectory.directory[randCardId];
      //clone into new card
      var cardClone = new Card();
      for(var k in directoryCard) cardClone[k]=directoryCard[k];

      cardClone.id = this.id++;

      deck.push(cardClone);
    }

    action.cards = deck.length;

    queue.complete(action);
    this.log.info('deck of %s cards for player %s',
      deck.length, action.playerId);
  }
}
