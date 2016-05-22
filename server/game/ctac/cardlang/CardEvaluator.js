import _ from 'lodash';
import loglevel from 'loglevel-decorator';
import EvalError from './EvalError.js';
import Position from '../models/Position.js';
import Statuses from '../models/Statuses.js';
import DrawCard from '../actions/DrawCard.js';
import DiscardCard from '../actions/DiscardCard.js';
import Message from '../actions/Message.js';
import CharmPiece from '../actions/CharmPiece.js';
import SetPlayerResource from '../actions/SetPlayerResource.js';
import PieceHealthChange from '../actions/PieceHealthChange.js';
import PieceDestroyed from '../actions/PieceDestroyed.js';
import PieceStatusChange from '../actions/PieceStatusChange.js';
import PieceAttributeChange from '../actions/PieceAttributeChange.js';
import SpawnPiece from '../actions/SpawnPiece.js';
import PieceBuff from '../actions/PieceBuff.js';
import PieceAura from '../actions/PieceAura.js';

/**
 * Evaluate the scripts on cards
 */
@loglevel
export default class CardEvaluator{
  constructor(queue, selector, pieceState, mapState){
    this.queue = queue;
    this.selector = selector;
    this.pieceState = pieceState;
    this.mapState = mapState;
    this.log.info('piece state %j', pieceState);
    this.startTurnTimers = [];
    this.endTurnTimers = [];

    this.eventDefaultSelectors = {
      playMinion: {left: 'SELF'},
      death: {left: 'SELF'},
      damaged: {left: 'SELF'},
      attacks: {left: 'SELF'},
      ability: {left: 'SELF'},
      cardDrawn: {left: 'PLAYER'},
      playSpell: {left: 'PLAYER'}
    };
    this.targetableActions = ['Hit', 'Heal', 'SetAttribute', 'Buff', 'GiveStatus', 'RemoveStatus', 'Charm', 'Destroy'];
    this.targetableEvents = ['playMinion', 'playSpell'];
  }

  //evaluate an event that directly relates to a piece, i.e. the piece dies
  evaluatePieceEvent(event, activatingPiece, targetPieceId, position, pivotPosition){
    this.log.info('Eval piece event %s activating piece: %j', event, activatingPiece);
    let evalActions = [];

    if(event === 'death'){
      this.cleanupTimers(activatingPiece);
    }

    //in the pieces case, if this is a spawn piece event the evaluator has the chance to return false
    //and scrub the spawn of the piece, so the activating piece isn't in the piece state yet.
    //However, include it in the loop so its events will be evaluated
    let pieces = new Set([...this.pieceState.pieces, activatingPiece]);

    //first look through all the pieces on the board to see if any have actions on this event
    for(let piece of pieces){
      if(piece.statuses & Statuses.Silence) continue;

      if(!piece.events || piece.events.length === 0) continue;

      //find all actions for this event, there could be more than one
      for(let cardEvent of piece.events){
        if(cardEvent.event !== event) continue;

        //see if the selector matches up for this piece
        let eventSelector = this.eventSelector(cardEvent);
        if(!eventSelector){
          eventSelector = this.eventDefaultSelectors[event];
          if(!eventSelector){
            throw 'Need default selector for ' + event;
          }
        }

        //now find all pieces that match the selector given the context of the piece that the event is for
        let piecesSelected = this.selector.selectPieces(piece.playerId, eventSelector, {selfPiece: piece, activatingPiece});

        let selectorMatched = piecesSelected.indexOf(activatingPiece) > -1;

        //if the activating piece is part of the selected pieces, add it to the list of actions
        if(selectorMatched){
          for(let cardEventAction of cardEvent.actions){
            evalActions.push({
              piece: piece,
              playerId: activatingPiece.playerId,
              position,
              pivotPosition,
              action: cardEventAction
            });
          }
        }
      }
    }

    return this.processActions(evalActions, activatingPiece, targetPieceId);

  }

  //evaluate an event that doesn't correspond to a piece directly
  evaluatePlayerEvent(event, playerId){
    this.log.info('Eval player event %s player: %s', event, playerId);

    let evalActions = [];

    //special handling of turn start and end for timers
    if(event === 'turnStart'){
      evalActions = evalActions.concat(this.updateTimers(true));
    }
    if(event === 'turnEnd'){
      evalActions = evalActions.concat(this.updateTimers(false));
    }

    //look through all the pieces on the board to see if any have actions on this event
    for(let piece of this.pieceState.pieces){
      if(piece.statuses & Statuses.Silence) continue;

      if(!piece.events || piece.events.length === 0) continue;

      //find all actions for this event, there could be more than one
      for(let cardEvent of piece.events){
        if(cardEvent.event !== event) continue;

        //see if the selector matches up for this piece
        let eventSelector = this.eventSelector(cardEvent);
        if(!eventSelector){
          eventSelector = this.eventDefaultSelectors[event];
          if(!eventSelector){
            throw 'Need default selector for ' + event;
          }
        }

        //now find the player that matches the selector given the context of the piece that the event is for
        let playerSelected = this.selector.selectPlayer(piece.playerId, eventSelector);

        //if the activating piece is part of the selected pieces, add it to the list of actions
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

    return this.processActions(evalActions);
  }

  //when a spell is played
  evaluateSpellEvent(event, {spellCard, playerId, targetPieceId, position, pivotPosition}){
    this.log.info('Eval spell event %s with spell %s player: %s', event, spellCard.name, playerId);

    let evalActions = [];

    //always add the actions from this spell card
    let spellActions = spellCard.events.filter(e => e.event === event);
    if(spellActions.length > 0){
      for(let cardEventAction of spellActions[0].actions){
        evalActions.push({
          piece: null,
          card: spellCard,
          playerId,
          position,
          pivotPosition,
          action: cardEventAction
        });
      }
    }

    //then look through all the pieces on the board to see if any have actions on this event
    for(let piece of this.pieceState.pieces){
      if(piece.statuses & Statuses.Silence) continue;

      if(!piece.events || piece.events.length === 0) continue;

      //find all actions for this event, there could be more than one
      for(let cardEvent of piece.events){
        if(cardEvent.event !== event) continue;

        //see if the selector matches up for this card
        let eventSelector = this.eventSelector(cardEvent);
        if(!eventSelector){
          eventSelector = this.eventDefaultSelectors[event];
          if(!eventSelector){
            throw 'Need default selector for ' + event;
          }
        }

        //now find the player that matches the selector given the context of the piece that the event is for
        let playerSelected = this.selector.selectPlayer(piece.playerId, eventSelector);

        //if the activating piece is part of the selected pieces, add it to the list of actions
        if(playerSelected === playerId){
          for(let cardEventAction of cardEvent.actions){
            evalActions.push({
              piece: piece,
              playerId: piece.playerId,
              action: cardEventAction,
              position,
              pivotPosition
            });
          }
        }
      }
    }

    return this.processActions(evalActions, null, targetPieceId);
  }

  //Process all actions that have been selected in the evaluation phase into actual queue actions
  // evalActions -> array of actions to be eval'd, with playerId's of the controlling player (current turn player)
  // activatingPiece -> optional piece that will be used for SELF selections
  // targetPieceId -> id of piece that's been targeted by spell/playMinion event
  processActions(evalActions, activatingPiece, targetPieceId){
    try{
      //see Spawn
      let spawnLocations = [];

      //Every time something selects pieces, save them for a potential timer to capture as the saved pieces
      let lastSelected = null;

      for(let pieceAction of evalActions){
        let action = pieceAction.action;
        let piece = pieceAction.piece;
        let card = pieceAction.card;
        let pieceSelectorParams = {
          selfPiece: piece,
          activatingPiece,
          targetPieceId,
          savedPieces: pieceAction.savedPieces,
          position: pieceAction.position,
          pivotPosition: pieceAction.pivotPosition,
          isSpell: !pieceAction.piece
        };
        let times = 1;
        if(action.times){
          times = this.selector.eventualNumber(action.times, pieceAction.playerId, pieceSelectorParams);
        }

        let actionTriggerer = piece ? `piece ${piece.name}` : `spell ${pieceAction.card.name}`;
        this.log.info('Evaluating action %s for %s %s %s'
          , action.action, actionTriggerer, times, times > 1 ? 'times' : 'time');

        for (var t = 0; t < times; t++) {
          switch(action.action){
            //Charm(pieceSelector)
            case 'Charm':
            {
              lastSelected = this.selector.selectPieces(pieceAction.playerId, action.args[0], pieceSelectorParams);
              this.log.info('Charm Selected %j', lastSelected);
              if(lastSelected && lastSelected.length > 0){
                for(let s of lastSelected){
                  this.queue.push(new CharmPiece(s.id));
                }
              }
              break;
            }
            //Destroy(pieceSelector)
            case 'Destroy':
            {
              lastSelected = this.selector.selectPieces(pieceAction.playerId, action.args[0], pieceSelectorParams);
              this.log.info('Destroyed Selected %j', lastSelected);
              if(lastSelected && lastSelected.length > 0){
                for(let s of lastSelected){
                  this.queue.push(new PieceDestroyed(s.id));
                }
              }
              break;
            }
            //Discard(playerSelector)
            //Only chooses a random card for now
            case 'Discard':
            {
              let playerSelector = this.selector.selectPlayer(pieceAction.playerId, action.args[0]);
              this.queue.push(new DiscardCard(playerSelector));
              break;
            }
            //DrawCard(playerSelector)
            case 'DrawCard':
            {
              let playerSelector = this.selector.selectPlayer(pieceAction.playerId, action.args[0]);
              this.queue.push(new DrawCard(playerSelector));
              break;
            }
            //ChangeEnergy(playerSelector, amount, permanent, full)
            //Permanent meaning for every turn going forward, non permanent is just this turn
            //Full meaning whether or not to fill the adjusted amount or if they come empty
            case 'ChangeEnergy':
            {
              let playerSelector = this.selector.selectPlayer(pieceAction.playerId, action.args[0]);
              this.queue.push(new SetPlayerResource(
                playerSelector,
                this.selector.eventualNumber(action.args[1]),
                action.args[2],
                action.args[3]
              ));
              break;
            }
            //Hit(pieceSelector, damageAmount)
            case 'Hit':
            {
              lastSelected = this.selector.selectPieces(pieceAction.playerId, action.args[0], pieceSelectorParams);
              this.log.info('Hit Selected %j', lastSelected);
              if(lastSelected && lastSelected.length > 0){
                for(let s of lastSelected){
                  this.queue.push(new PieceHealthChange(s.id, -this.selector.eventualNumber(action.args[1], pieceAction.playerId, pieceSelectorParams)));
                }
              }
              break;
            }
            //Heal(pieceSelector, healAmount)
            case 'Heal':
            {
              lastSelected = this.selector.selectPieces(pieceAction.playerId, action.args[0], pieceSelectorParams);
              this.log.info('Heal Selected %j', lastSelected);
              if(lastSelected && lastSelected.length > 0){
                for(let s of lastSelected){
                  this.queue.push(new PieceHealthChange(s.id, this.selector.eventualNumber(action.args[1], pieceAction.playerId, pieceSelectorParams)));
                }
              }
              break;
            }
            //SetAttribute(pieceSelector, attribute, value)
            case 'SetAttribute':
            {
              lastSelected = this.selector.selectPieces(pieceAction.playerId, action.args[0], pieceSelectorParams);
              this.log.info('Set Attr Selected %j', lastSelected);
              if(lastSelected && lastSelected.length > 0){
                for(let s of lastSelected){
                  let phc = new PieceAttributeChange(s.id);
                  //set up the appropriate attribute change from args, i.e. attack = 1
                  phc[action.args[1]] = this.selector.eventualNumber(action.args[2], pieceAction.playerId, pieceSelectorParams);
                  this.queue.push(phc);
                }
              }

              break;
            }
            //Buff(pieceSelector, buffName, attribute(amount), ...moreAttributes)
            case 'Buff':
            {
              let buffName = action.args[1];
              lastSelected = this.selector.selectPieces(pieceAction.playerId, action.args[0], pieceSelectorParams);
              this.log.info('Buff Selected %j', lastSelected);
              let buffAttributes = action.args.slice(2);

              if(lastSelected && lastSelected.length > 0){
                for(let s of lastSelected){
                  //set up a new buff for each selected piece that has all the attributes of the buff
                  let buff = new PieceBuff(s.id, buffName, false);
                  for(let buffAttribute of buffAttributes){
                    buff[buffAttribute.attribute] = this.selector.eventualNumber(buffAttribute.amount, pieceAction.playerId, pieceSelectorParams);
                  }
                  this.queue.push(buff);
                }
              }
              break;
            }
            //Aura(auraPieceSelector, pieceSelector, auraName, attribute(amount), ...moreAttributes)
            //to clarify further: first piece selector is who to attach the aura to,
            //second one is who is affected by the aura
            case 'Aura':
            {
              lastSelected = this.selector.selectPieces(pieceAction.playerId, action.args[0], pieceSelectorParams);
              this.log.info('Aura Selected %j', lastSelected);
              let pieceSelector = action.args[1];
              let auraName = action.args[2];
              let auraAttributes = action.args.slice(3);

              if(lastSelected && lastSelected.length > 0){
                for(let s of lastSelected){
                  //set up a new aura for each selected piece that has all the attributes of the aura
                  let aura = new PieceAura(s.id, pieceSelector, auraName);
                  for(let auraAttribute of auraAttributes){
                    aura[auraAttribute.attribute] = this.selector.eventualNumber(auraAttribute.amount, pieceAction.playerId, pieceSelectorParams);
                  }
                  this.queue.push(aura);
                }
              }
              break;
            }
            //RemoveBuff(pieceSelector, buffName)
            case 'RemoveBuff':
            {
              let buffName = action.args[1];
              lastSelected = this.selector.selectPieces(pieceAction.playerId, action.args[0], pieceSelectorParams);
              this.log.info('Remove Buff Selected %j', lastSelected);

              if(lastSelected && lastSelected.length > 0){
                for(let s of lastSelected){
                  let buff = new PieceBuff(s.id, buffName, true);
                  this.queue.push(buff);
                }
              }
              break;
            }
            //Spawn(pieceId, kingsRadiusToSpawnIn)
            //Spawn a unit based on where the activating piece is located.  So if the kings radius is 0
            //spawn it right where the piece was (after it died presumably).  If it's 1, pick a random position
            //from any of the surrounding tiles
            //Also check to make sure it's a valid spawn location and another loop hasn't spawned at the same location
            case 'Spawn':
            {
              let possiblePositions = this.mapState.getKingTilesInRadius(piece.position, action.args[1]);
              possiblePositions = _.chain(possiblePositions)
                .filter(p => (p.tileEquals(piece.position) || !this.pieceState.pieceAt(p.x, p.z) )
                  && !spawnLocations.find(s => s.equals(p)))
                .value();
              if(possiblePositions.length > 0){
                let position = _.sample(possiblePositions);
                spawnLocations.push(position);
                let spawn = new SpawnPiece(piece.playerId, null, action.args[0], position, null);
                this.queue.push(spawn);
              }else{
                this.log.info('Couldn\'t spawn piece because there\'s no where to put it');
              }

              break;
            }

            //GiveStatus(pieceSelector, Status)
            case 'GiveStatus':
            {
              lastSelected = this.selector.selectPieces(pieceAction.playerId, action.args[0], pieceSelectorParams);
              this.log.info('Give Status Selected %j status %s', lastSelected, action.args[1]);
              if(lastSelected && lastSelected.length > 0){
                for(let s of lastSelected){
                  this.queue.push(new PieceStatusChange(s.id, Statuses[action.args[1]] ));
                }
              }
              break;
            }
            //RemoveStatus(pieceSelector, Status)
            case 'RemoveStatus':
            {
              lastSelected = this.selector.selectPieces(pieceAction.playerId, action.args[0], pieceSelectorParams);
              this.log.info('Remove Status Selected %j status %s', lastSelected, action.args[1]);
              if(lastSelected && lastSelected.length > 0){
                for(let s of lastSelected){
                  this.queue.push(new PieceStatusChange(s.id, null, Statuses[action.args[1]] ));
                }
              }
              break;
            }
            //startTurnTimer(turns, repeating, ...Actions)
            case 'startTurnTimer':
            {
              let timerActions = action.args.slice(2);
              if(timerActions && timerActions.length > 0){
                for(let timerAction of timerActions){
                  this.startTurnTimers.push({
                    saved: lastSelected,
                    piece,
                    card,
                    playerId: pieceAction.playerId,
                    interval: action.args[0],
                    timer: action.args[0],
                    repeating: action.args[1],
                    timerAction: timerAction
                  });
                }
              }
              break;
            }
            //endTurnTimer(turns, repeating, ...Actions)
            case 'endTurnTimer':
            {
              let timerActions = action.args.slice(2);
              if(timerActions && timerActions.length > 0){
                for(let timerAction of timerActions){
                  this.endTurnTimers.push({
                    saved: lastSelected,
                    piece,
                    card,
                    playerId: pieceAction.playerId,
                    interval: action.args[0],
                    timer: action.args[0],
                    repeating: action.args[1],
                    timerAction: timerAction
                  });
                }
              }
              break;
            }
          }
        }
      }
    }catch(e){
      if(e instanceof EvalError){
        this.queue.push(new Message(e.message));
        return false;
      }
      throw e;
    }
    return true;
  }

  //Tick down all the timers in the array. When the metaphorical bomb ticks down remove it from
  //the saved timers and return its actions to be evaluated
  //have to use a bool instead of passing the array in so we can reassign it to remove items :(
  updateTimers(isStartTimers){
    let timerArray = isStartTimers ? this.startTurnTimers : this.endTurnTimers;
    if(timerArray.length === 0) return [];

    for(let timer of timerArray){
      timer.timer--;
    }
    let activatedTimers = timerArray.filter(t => t.timer === 0);

    //reactivate those that are repeating
    for(let activated of activatedTimers){
      if(activated.repeating){
        activated.timer = activated.interval;
      }
    }

    //refilter for expired
    let expiredTimers = timerArray.filter(t => t.timer === 0);

    if(isStartTimers){
      this.startTurnTimers = _.without(timerArray, ...expiredTimers);
    }else{
      this.endTurnTimers = _.without(timerArray, ...expiredTimers);
    }

    let activatedEvents = [];

    for(let activatedTimer of activatedTimers){
      activatedEvents.push({
        piece: activatedTimer.piece,
        card: activatedTimer.card,
        savedPieces: activatedTimer.saved,
        playerId: activatedTimer.playerId,
        action: activatedTimer.timerAction
      });
    }

    return activatedEvents;
  }

  //Whenever a piece dies that had a repeating turn timer on it we need to remove it
  cleanupTimers(killedPiece){
    let abandonedFilter = (t) => (t.repeating == true && t.piece && t.piece.id === killedPiece.id);

    let abandonedStartTimers = this.startTurnTimers.filter(abandonedFilter);
    if(abandonedStartTimers.length > 0){
      this.log.info('Cleaning up %s start timers', abandonedStartTimers.length);
    }
    this.startTurnTimers = _.without(this.startTurnTimers, ...abandonedStartTimers);

    let abandonedEndTimers = this.endTurnTimers.filter(abandonedFilter);
    if(abandonedEndTimers.length > 0){
      this.log.info('Cleaning up %s end timers', abandonedEndTimers.length);
    }
    this.endTurnTimers = _.without(this.endTurnTimers, ...abandonedEndTimers);
  }

  //look through the cards for any cards needing a TARGET
  //on one of the targetableEvents
  //and then find what the possible targets are
  //   [
  //     {cardId: 2, event: 'x', targetPieceIds: [4,5,6]}
  //   ]
  findPossibleTargets(cards, playerId){
    let targets = [];

    for(let card of cards){
      if(!card.events) continue;

      for(let targetEvent of this.targetableEvents){
        let event = card.events.find(e => e.event === targetEvent);
        if(!event) continue;

        let targetPieceIds = this.findActionTargets(event.actions, playerId, card.tags.includes('Spell'));
        if(targetPieceIds){
          targets.push({
            cardId: card.id,
            event: event.event,
            targetPieceIds
          });
        }
      }
    }

    return targets;
  }

  findActionTargets(eventActions, playerId, isSpell){

    //try to find TARGETS in any of the actions
    for(let cardEventAction of eventActions){
      if(this.targetableActions.indexOf(cardEventAction.action) === -1) continue;
      //ASSUMING ACTION SELECTORS ARE ALWAYS THE FIRST ARG
      let selector = cardEventAction.args[0];

      if(!this.selector.doesSelectorUse(selector, 'TARGET')) continue;

      let targetPieceIds = this.selector.selectPossibleTargets(
        playerId, selector, isSpell
      ).map(p => p.id);

      //only allow max of 1 targetable action per event
      return targetPieceIds;
    }

    return null;
  }

  //look through the pieces for any piece with an ability
  //then find the name, charge time, and what the possible targets are
  //   [
  //     {pieceId: 2, ability: 'x', abilityCost: 0, abilityChargeTime: 0, abilityCooldown: 0, targetPieceIds: [4,5,6]}
  //   ]
  findPossibleAbilities(pieces, playerId){
    let targets = [];

    let playerPieces = pieces.filter(x => x.playerId === playerId);

    for(let piece of playerPieces){
      if(!piece.events) continue;

      let ability = piece.events.find(e => e.event === 'ability');

      if(!ability) continue;

      let targetPieceIds = this.findActionTargets(ability.actions, playerId, true) || [];
      targets.push({
        pieceId: piece.id,
        abilityCost: ability.args[0],
        abilityChargeTime: ability.args[1],
        abilityCooldown: Math.max(0, ability.args[1] - piece.abilityCharge),
        ability: ability.args[2],
        targetPieceIds
      });
    }

    return targets;
  }

  //look through cards for any areas when played
  //   [
  //     {cardId: 2, event: 'x', area: [Square|Cross...], size: 3, center?: Position}
  //   ]
  findPossibleAreas(cards, playerId){
    let areas = [];

    for(let card of cards){
      if(!card.events) continue;

      for(let targetEvent of this.targetableEvents){
        let event = card.events.find(e => e.event === targetEvent);
        if(!event) continue;

        let isSpell = card.tags.includes('Spell');
        let areaSelector = null;
        //try to find areas in any of the actions
        for(let cardEventAction of event.actions){
          if(this.targetableActions.indexOf(cardEventAction.action) === -1) continue;
          //ASSUMING ACTION SELECTORS ARE ALWAYS THE FIRST ARG
          let selector = cardEventAction.args[0];

          areaSelector = this.selector.findSelector(selector, s => s && s.area);
          if(!areaSelector) continue;

          //only allow max of 1 area action per event
          break;
        }

        if(areaSelector){
          let areaDescrip = this.selector.selectArea(
            playerId,
            areaSelector,
            {isSpell: card.tags.includes('Spell')}
          );

          areas.push({
            cardId: card.id,
            event: event.event,
            areaType: areaDescrip.areaType,
            size: areaDescrip.size,
            isCursor: areaDescrip.isCursor,
            isDoubleCursor: areaDescrip.isDoubleCursor,
            bothDirections: areaDescrip.bothDirections,
            selfCentered: areaDescrip.selfCentered,
            centerPosition: areaDescrip.centerPosition,
            pivotPosition: areaDescrip.pivotPosition,
            areaTiles: areaDescrip.areaTiles
          });
        }
      }
    }

    return areas;
  }

  //Gets the event selector for the card event.  By convention this is the 0th arg that's a selector
  //but the 0th arg doesn't have to be a selector
  eventSelector(cardEvent){
    if(!cardEvent.args) return null;

    const firstArg = cardEvent.args[0];
    if(firstArg.left){
      return firstArg;
    }
    return null;
  }
}
