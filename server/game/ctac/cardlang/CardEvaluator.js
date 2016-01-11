import _ from 'lodash';
import loglevel from 'loglevel-decorator';
import DrawCard from '../actions/DrawCard.js';
import PieceHealthChange from '../actions/PieceHealthChange.js';
import PieceAttributeChange from '../actions/PieceAttributeChange.js';
import PieceBuff from '../actions/PieceBuff.js';

/**
 * Evaluate the scripts on cards
 */
@loglevel
export default class CardEvaluator{
  constructor(queue, selector, cardDirectory, pieceState){
    this.queue = queue;
    this.selector = selector;
    this.cardDirectory = cardDirectory;
    this.pieceState = pieceState;
    this.log.info('piece state %j', pieceState);

    this.eventDefaultSelectors = {
      playMinion: {left: 'SELF'},
      death: {left: 'SELF'},
      damaged: {left: 'SELF'},
      attacks: {left: 'SELF'},
      cardDrawn: {left: 'PLAYER'},
      playSpell: {left: 'PLAYER'}
    };
  }

  //evaluate an event that directly relates to a piece, i.e. the piece dies
  evaluatePieceEvent(event, triggeringPiece){
    this.log.info('Eval piece event %s triggering piece: %j', event, triggeringPiece);
    let evalActions = [];

    //first look through all the pieces on the board to see if any have actions on this event
    for(let piece of this.pieceState.pieces){
      let card = this.cardDirectory.directory[piece.cardId];
      if(!card.events || card.events.length === 0) continue;

      //find all actions for this event, there could be more than one
      for(let cardEvent of card.events){
        if(cardEvent.event !== event) continue;

        //see if the selector matches up for this card
        let eventSelector = cardEvent.selector;
        if(!eventSelector){
          eventSelector = this.eventDefaultSelectors[event];
          if(!eventSelector){
            throw 'Need default selector for ' + event;
          }
        }

        //now find all pieces that match the selector given the context of the piece that the event is for
        let piecesSelected = this.selector.selectPieces(piece.playerId, eventSelector, piece);

        let selectorMatched = piecesSelected.indexOf(triggeringPiece) > -1;

        //if the triggering piece is part of the selected pieces, add it to the list of actions
        if(selectorMatched){
          for(let cardEventAction of cardEvent.actions){
            evalActions.push({
              piece: piece,
              playerId: triggeringPiece.playerId,
              action: cardEventAction
            });
          }
        }
      }
    }

    this.processActions(evalActions, triggeringPiece);

  }

  //evaluate an event that doesn't correspond to a piece directly
  evaluatePlayerEvent(event, playerId){
    this.log.info('Eval player event %s player: %s', event, playerId);

    let evalActions = [];

    //first look through all the pieces on the board to see if any have actions on this event
    for(let piece of this.pieceState.pieces){
      let card = this.cardDirectory.directory[piece.cardId];
      if(!card.events || card.events.length === 0) continue;

      //find all actions for this event, there could be more than one
      for(let cardEvent of card.events){
        if(cardEvent.event !== event) continue;

        //see if the selector matches up for this card
        let eventSelector = cardEvent.selector;
        if(!eventSelector){
          eventSelector = this.eventDefaultSelectors[event];
          if(!eventSelector){
            throw 'Need default selector for ' + event;
          }
        }

        //now find the player that matches the selector given the context of the piece that the event is for
        let playerSelected = this.selector.selectPlayer(piece.playerId, eventSelector);

        //if the triggering piece is part of the selected pieces, add it to the list of actions
        if(playerSelected === playerId){
          for(let cardEventAction of cardEvent.actions){
            evalActions.push({
              piece: piece,
              playerId: piece.playerId,
              action: cardEventAction
            });
          }
        }
      }
    }

    this.processActions(evalActions);
  }

  //when a spell is played
  evaluateSpellEvent(event, spellCard, playerId){
    this.log.info('Eval spell event %s with spell %s player: %s', event, spellCard.name, playerId);

    let evalActions = [];

    //always add the actions from this spell card
    let spellActions = spellCard.events.filter(e => e.event === event);
    if(spellActions.length > 0){
      for(let cardEventAction of spellActions[0].actions){
        evalActions.push({
          card: spellCard,
          playerId: playerId,
          action: cardEventAction
        });
      }
    }

    //then look through all the pieces on the board to see if any have actions on this event
    for(let piece of this.pieceState.pieces){
      let card = this.cardDirectory.directory[piece.cardId];
      if(!card.events || card.events.length === 0) continue;

      //find all actions for this event, there could be more than one
      for(let cardEvent of card.events){
        if(cardEvent.event !== event) continue;

        //see if the selector matches up for this card
        let eventSelector = cardEvent.selector;
        if(!eventSelector){
          eventSelector = this.eventDefaultSelectors[event];
          if(!eventSelector){
            throw 'Need default selector for ' + event;
          }
        }

        //now find the player that matches the selector given the context of the piece that the event is for
        let playerSelected = this.selector.selectPlayer(piece.playerId, eventSelector);

        //if the triggering piece is part of the selected pieces, add it to the list of actions
        if(playerSelected === playerId){
          for(let cardEventAction of cardEvent.actions){
            evalActions.push({
              piece: piece,
              playerId: piece.playerId,
              action: cardEventAction
            });
          }
        }
      }
    }

    this.processActions(evalActions);
  }

  //Process all actions that have been selected in the evaluation phase into actual queue actions
  // evalActions -> array of actions to be eval'd, with playerId's of the controlling player (current turn player)
  // triggeringPiece -> optional piece that will be used for SELF selections
  processActions(evalActions, triggeringPiece){
    for(let pieceAction of evalActions){
      let action = pieceAction.action;
      let times = 1;
      if(action.times){
        times = this.eventualNumber(action.times);
      }

      let actionTriggerer = pieceAction.piece ? `piece ${pieceAction.piece.name}` : `spell ${pieceAction.card.name}`;
      this.log.info('Evaluating action %s for %s %s %s'
        , action.action, actionTriggerer, times, times > 1 ? 'times' : 'time');

      for (var t = 0; t < times; t++) {
        switch(action.action){
          case 'DrawCard':
          {
            let playerSelector = this.selector.selectPlayer(pieceAction.playerId, action.args[0]);
            this.queue.push(new DrawCard(playerSelector));
            break;
          }
          case 'Hit':
          {
            let selected = this.selector.selectPieces(pieceAction.playerId, action.args[0], triggeringPiece);
            this.log.info('Selected %j', selected);
            if(selected && selected.length > 0){
              for(let s of selected){
                this.queue.push(new PieceHealthChange(s.id, -action.args[1]));
              }
            }
            break;
          }
          case 'Heal':
          {
            let selected = this.selector.selectPieces(pieceAction.playerId, action.args[0], triggeringPiece);
            this.log.info('Selected %j', selected);
            if(selected && selected.length > 0){
              for(let s of selected){
                this.queue.push(new PieceHealthChange(s.id, action.args[1]));
              }
            }
            break;
          }
          case 'SetAttribute':
          {
            let selected = this.selector.selectPieces(pieceAction.playerId, action.args[0], triggeringPiece);
            this.log.info('Selected %j', selected);
            if(selected && selected.length > 0){
              for(let s of selected){
                let phc = new PieceAttributeChange(s.id);
                //set up the appropriate attribute change from args, i.e. attack = 1
                phc[action.args[1]] = action.args[2];
                this.queue.push(phc);
              }
            }

            break;
          }
          case 'Buff':
          {
            let buffName = action.args[0];
            let selected = this.selector.selectPieces(pieceAction.playerId, action.args[1], triggeringPiece);
            this.log.info('Selected %j', selected);
            let buffAttributes = action.args.splice(2);

            if(selected && selected.length > 0){
              for(let s of selected){
                //set up a new buff for each selected piece that has all the attributes of the buff
                let buff = new PieceBuff(s.id, buffName);
                for(let buffAttribute of buffAttributes){
                  buff[buffAttribute.attribute] = buffAttribute.amount;
                }
                this.queue.push(buff);
              }
            }
            break;
          }
        }
      }
    }
  }

  //can either be an ordinary number, or something that evaluates to a number
  eventualNumber(input){
    if(input.randList){
      return _.sample(input.randList);
    }
    return input;
  }
}
