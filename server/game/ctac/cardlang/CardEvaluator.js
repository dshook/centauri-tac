import loglevel from 'loglevel-decorator';
import DrawCard from '../actions/DrawCard.js';
import PieceHealthChange from '../actions/PieceHealthChange.js';
import PieceAttributeChange from '../actions/PieceAttributeChange.js';

/**
 * Evaluate the scripts on cards
 */
@loglevel
export default class CardEvaluator{
  constructor(queue, selector, cardDirectory){
    this.queue = queue;
    this.selector = selector;
    this.cardDirectory = cardDirectory;
  }

  evaluateAction(event, piece){
    let card = this.cardDirectory.directory[piece.cardId];

    this.log.info('Eval event %s piece %j: %j', event, piece, card.events);
    if(!card.events || !card.events[event]) return;
    let evalActions = card.events[event];

    for (let i = 0; i < evalActions.length; i++) {
      let action = evalActions[i];
      let times = 1;
      if(action.times){
        times = action.times;
      }

      this.log.info('Evaluating action %s for card %s %s %s'
        , action.action, card.name, times, times > 1 ? 'times' : 'time');

      for (var t = 0; t < times; t++) {
        switch(action.action){
          case 'DrawCard':
          {
            let playerSelector = this.selector.selectPlayer(piece.playerId, action.args[0]);
            this.queue.push(new DrawCard(playerSelector));
            break;
          }
          case 'Hit':
          {
            let selected = this.selector.selectPieces(piece.playerId, action.args[0]);
            this.log.info('Selected %j', selected);
            if(selected && selected.length > 0){
              for(let s of selected){
                this.queue.push(new PieceHealthChange(s.id, -action.args[1]));
              }
            }
            break;
          }
          case 'SetAttribute':
          {
            let selected = this.selector.selectPieces(piece.playerId, action.args[0]);
            this.log.info('Selected %j', selected);
            if(selected && selected.length > 0){
              for(let s of selected){
                var phc = new PieceAttributeChange(s.id);
                //set up the appropriate attribute change from args, i.e. attack = 1
                phc[action.args[1]] = action.args[2];
                this.queue.push(phc);
              }
            }

            break;
          }
        }
      }
    };
  }
}