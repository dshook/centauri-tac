import _ from 'lodash';
import loglevel from 'loglevel-decorator';
import EvalError from './EvalError.js';
import Statuses from '../models/Statuses.js';
import DrawCard from '../actions/DrawCard.js';
import DiscardCard from '../actions/DiscardCard.js';
import GiveCard from '../actions/GiveCard.js';
import ShuffleToDeck from '../actions/ShuffleToDeck.js';
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
import CardAura from '../actions/CardAura.js';
import MovePiece from '../actions/MovePiece.js';
import TransformPiece from '../actions/TransformPiece.js';
import PieceArmorChange from '../actions/PieceArmorChange.js';
import AttachCode from '../actions/AttachCode.js';
import UnsummonPiece from '../actions/UnsummonPiece.js';
import Choose from '../actions/Choose.js';

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
    this.startTurnTimers = [];
    this.endTurnTimers = [];

    this.eventDefaultSelectors = {
      playMinion: {left: 'SELF'},
      death: {left: 'SELF'},
      damaged: {left: 'SELF'},
      healed: {left: 'SELF'},
      attacks: {left: 'SELF'},
      ability: {left: 'SELF'},
      cardDrawn: {left: 'PLAYER'},
      playSpell: {left: 'PLAYER'}
    };
    this.targetableActions = [
      'Hit',
      'Heal',
      'SetAttribute',
      'Buff',
      'GiveStatus',
      'RemoveStatus',
      'Charm',
      'Destroy',
      'Move',
      'Transform',
      'GiveCard',
      'ShuffleToDeck',
      'Unsummon',
      'startTurnTimer',
      'endTurnTimer'
    ];
    this.targetableEvents = ['playMinion', 'playSpell'];
  }

  //evaluate an event that directly relates to a piece, i.e. the piece dies
  evaluatePieceEvent(event, activatingPiece, pieceEventParams){
    this.log.info('Eval piece event %s activating piece: %j', event, activatingPiece);
    let evalActions = [];

    let {targetPieceId, position, pivotPosition, chooseCardTemplateId} = pieceEventParams || {};

    if(event === 'death'){
      this.cleanupTimers(activatingPiece);
    }

    //in the pieces case, if this is a spawn piece event the evaluator has the chance to return false
    //and scrub the spawn of the piece, so the activating piece isn't in the piece state yet.
    //However, include it in the loop so its events will be evaluated
    let pieces = new Set([...this.pieceState.pieces, activatingPiece]);

    //first look through all the pieces on the board to see if any have actions on this event
    for(let piece of pieces){
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
        let piecesSelected = this.selector.selectPieces(eventSelector,
          {selfPiece: piece, activatingPiece, controllingPlayerId: piece.playerId});

        let selectorMatched = piecesSelected.indexOf(activatingPiece) > -1;

        //if the activating piece is part of the selected pieces, add it to the list of actions
        if(selectorMatched){
          for(let cardEventAction of cardEvent.actions){
            evalActions.push({
              piece: piece,
              playerId: activatingPiece.playerId,
              position,
              pivotPosition,
              targetPieceId,
              action: cardEventAction,
              chooseCardTemplateId
            });
          }
        }
      }
    }

    return this.processActions(evalActions, activatingPiece, activatingPiece.playerId);

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

    return this.processActions(evalActions, null, playerId);
  }

  //when a spell is played
  evaluateSpellEvent(event,
    {spellCard, playerId, targetPieceId, position, pivotPosition, chooseCardTemplateId, activatingPiece, selfPiece}
  ){
    this.log.info('Eval spell event %s with spell %s player: %s', event, spellCard.name, playerId);

    let evalActions = [];

    //always add the actions from this spell card
    let spellActions = spellCard.events.filter(e => e.event === event);
    if(spellActions.length > 0){
      for(let cardEventAction of spellActions[0].actions){
        evalActions.push({
          piece: selfPiece || null,
          card: spellCard,
          playerId,
          position,
          pivotPosition,
          targetPieceId,
          action: cardEventAction,
          chooseCardTemplateId
        });
      }
    }

    //then look through all the pieces on the board to see if any have actions on this event
    for(let piece of this.pieceState.pieces){
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
              pivotPosition,
              targetPieceId,
              chooseCardTemplateId
            });
          }
        }
      }
    }

    return this.processActions(evalActions, activatingPiece || null, playerId);
  }

  //Process all actions that have been selected in the evaluation phase into actual queue actions
  // evalActions -> array of actions to be eval'd, with playerId's of the controlling player (current turn player)
  // activatingPiece -> optional piece that will be used for SELF selections
  // activatingPlayerId -> id of the player that fired all this off (generally the current turn player)
  processActions(evalActions, activatingPiece, activatingPlayerId){
    //actions to be added to the real queue
    let queue = [];

    try{
      let spawnLocations = []; //see Spawn
      let pieceDamages = {}; //se Hit

      //Every time something selects pieces, save them for a potential timer to capture as the saved pieces
      let lastSelected = null;

      for(let pieceAction of evalActions){
        let action = pieceAction.action;
        let piece = pieceAction.piece;
        let card = pieceAction.card;
        let pieceSelectorParams = {
          selfPiece: piece,
          controllingPlayerId: pieceAction.playerId,
          activatingPiece,
          targetPieceId: pieceAction.targetPieceId,
          savedPieces: pieceAction.savedPieces,
          position: pieceAction.position,
          pivotPosition: pieceAction.pivotPosition,
          isSpell: !pieceAction.piece,
          isTimer: pieceAction.isTimer || false
        };

        //check to see if we should even do this action
        if(action.condition){
          let compareResult = this.selector.compareExpression(action.condition, this.pieceState, pieceSelectorParams)
          if(compareResult.length === 0){
            continue;
          }
        }

        let times = 1;
        if(action.times){
          times = this.selector.eventualNumber(action.times, pieceSelectorParams);
        }
        let totalSpellDamage = this.pieceState.totalSpellDamage(pieceSelectorParams.controllingPlayerId);
        //for multi cast spells use spell damage as additional times instead of additional damage
        if(pieceSelectorParams.isSpell && times > 1 && totalSpellDamage > 0){
          times += totalSpellDamage;
        }

        let actionTriggerer = piece ? `piece ${piece.name}` : `spell ${pieceAction.card.name}`;
        this.log.info('Evaluating action %s for %s %s %s'
          , action.action, actionTriggerer, times, times > 1 ? 'times' : 'time');

        for (var time = 0; time < times; time++) {
          switch(action.action){
            //Charm(pieceSelector)
            case 'Charm':
            {
              lastSelected = this.selector.selectPieces(action.args[0], pieceSelectorParams);
              this.log.info('Charm Selected %j', lastSelected);
              if(lastSelected && lastSelected.length > 0){
                for(let s of lastSelected){
                  queue.push(new CharmPiece(s.id));
                }
              }
              break;
            }
            //Destroy(pieceSelector)
            case 'Destroy':
            {
              lastSelected = this.selector.selectPieces(action.args[0], pieceSelectorParams);
              this.log.info('Destroyed Selected %j', lastSelected);
              if(lastSelected && lastSelected.length > 0){
                for(let s of lastSelected){
                  queue.push(new PieceDestroyed(s.id));
                }
              }
              break;
            }
            //Discard(playerSelector)
            //Only chooses a random card for now
            case 'Discard':
            {
              let playerSelector = this.selector.selectPlayer(pieceAction.playerId, action.args[0]);
              queue.push(new DiscardCard(playerSelector));
              break;
            }
            //DrawCard(playerSelector)
            case 'DrawCard':
            {
              let playerSelector = this.selector.selectPlayer(pieceAction.playerId, action.args[0]);
              queue.push(new DrawCard(playerSelector));
              break;
            }
            //ChangeEnergy(playerSelector, amount, permanent, full)
            //Permanent meaning for every turn going forward, non permanent is just this turn
            //Full meaning whether or not to fill the adjusted amount or if they come empty
            case 'ChangeEnergy':
            {
              let playerSelector = this.selector.selectPlayer(pieceAction.playerId, action.args[0]);
              queue.push(new SetPlayerResource(
                playerSelector,
                this.selector.eventualNumber(action.args[1], pieceSelectorParams),
                action.args[2],
                action.args[3]
              ));
              break;
            }
            //Hit(pieceSelector, damageAmount)
            case 'Hit':
            {
              let spellDamageBonus = 0;
              let pieceSelector = action.args[0];
              let hitDamage = action.args[1];
              //apply spell damage only for one time cast spells
              if(pieceSelectorParams.isSpell && times === 1){
                spellDamageBonus = totalSpellDamage;
              }
              let damage = -(this.selector.eventualNumber(hitDamage, pieceSelectorParams) + spellDamageBonus);

              //for multi time hit spells, we will need to loop through all the pieces and determine which pieces
              //already would have died from previous damage and then not include them in the piece selection process
              if(times > 1){
                let deadPiecesToExclude = [];
                for(let piece of this.pieceState.pieces){
                  if(this.checkPieceDeath(pieceDamages, piece.id)){
                    deadPiecesToExclude.push(piece.id);
                  }
                }

                if(deadPiecesToExclude.length > 0){
                  this.log.info('Excluding these pieces since they died: %j', deadPiecesToExclude);
                  //to exclude, wrap the current selector to remove unwanted pieces, careful with random selections though
                  if(pieceSelector.random){
                    pieceSelector = {
                      random: true,
                      selector: {
                        left: pieceSelector.selector,
                        op: '-',
                        right: {
                          pieceIds: deadPiecesToExclude
                        }
                      }
                    };
                  }else{
                    pieceSelector = {
                      left: pieceSelector,
                      op: '-',
                      right: {
                        pieceIds: deadPiecesToExclude
                      }
                    };
                  }
                }
              }

              lastSelected = this.selector.selectPieces(pieceSelector, pieceSelectorParams);
              this.log.info('Hit Selected %s pieces damage %s (+%s)', lastSelected.length, -(damage - spellDamageBonus), spellDamageBonus);
              if(lastSelected && lastSelected.length > 0){
                for(let s of lastSelected){
                  this.addPieceDamageTaken(pieceDamages, s.id, damage);
                  queue.push(new PieceHealthChange(s.id, damage));
                }
              }
              break;
            }
            //Heal(pieceSelector, healAmount)
            case 'Heal':
            {
              lastSelected = this.selector.selectPieces(action.args[0], pieceSelectorParams);
              this.log.info('Heal Selected %j', lastSelected);
              if(lastSelected && lastSelected.length > 0){
                for(let s of lastSelected){
                  queue.push(new PieceHealthChange(s.id, this.selector.eventualNumber(action.args[1], pieceSelectorParams)));
                }
              }
              break;
            }
            //SetAttribute(pieceSelector, attribute, value)
            case 'SetAttribute':
            {
              lastSelected = this.selector.selectPieces(action.args[0], pieceSelectorParams);
              this.log.info('Set Attr Selected %j', lastSelected);
              if(lastSelected && lastSelected.length > 0){
                for(let s of lastSelected){
                  let phc = new PieceAttributeChange(s.id);
                  //set up the appropriate attribute change from args, i.e. attack = 1
                  phc[action.args[1]] = this.selector.eventualNumber(action.args[2], pieceSelectorParams);
                  queue.push(phc);
                }
              }

              break;
            }
            //Buff(pieceSelector, buffName, [optional condition expression], attribute(amount), ...moreAttributes)
            case 'Buff':
            {
              let buffName = action.args[1];
              lastSelected = this.selector.selectPieces(action.args[0], pieceSelectorParams);
              this.log.info('Buff Selected %j', lastSelected);

              let attributeIndex = 2;
              let condition = null;
              if(action.args[2] && action.args[2].compareExpression){
                attributeIndex = 3;
                condition = action.args[2];
              }
              let buffAttributes = action.args.slice(attributeIndex);

              if(lastSelected && lastSelected.length > 0){
                for(let s of lastSelected){
                  //set up a new buff for each selected piece that has all the attributes of the buff
                  let buff = new PieceBuff(s.id, buffName, false);
                  buff.condition = condition;
                  for(let buffAttribute of buffAttributes){
                    buff[buffAttribute.attribute] = this.selector.eventualNumber(buffAttribute.amount, pieceSelectorParams);
                  }
                  queue.push(buff);
                }
              }
              break;
            }
            //Aura(auraPieceSelector, pieceSelector, auraName, attribute(amount), ...moreAttributes)
            //to clarify further: first piece selector is who to attach the aura to,
            //second one is who is affected by the aura
            case 'Aura':
            {
              lastSelected = this.selector.selectPieces(action.args[0], pieceSelectorParams);
              this.log.info('Aura Selected %j', lastSelected);
              let pieceSelector = action.args[1];
              let auraName = action.args[2];
              let auraAttributes = action.args.slice(3);

              if(lastSelected && lastSelected.length > 0){
                for(let s of lastSelected){
                  //set up a new aura for each selected piece that has all the attributes of the aura
                  let aura = new PieceAura(s.id, pieceSelector, auraName);
                  for(let auraAttribute of auraAttributes){
                    //specifically don't use eventual number here because it will be evaluated in the aura update
                    aura[auraAttribute.attribute] = auraAttribute.amount;
                  }
                  queue.push(aura);
                }
              }
              break;
            }
            //CardAura(auraPieceSelector, cardSelector, auraName, attribute(amount), ...moreAttributes)
            //to clarify further: first piece selector is who to attach the aura to,
            //second one is which cards are affected by the aura.
            case 'CardAura':
            {
              lastSelected = this.selector.selectPieces(action.args[0], pieceSelectorParams);
              this.log.info('Card Aura Selected %j', lastSelected);
              let cardSelector = action.args[1];
              let auraName = action.args[2];
              let auraAttributes = action.args.slice(3);

              if(lastSelected && lastSelected.length > 0){
                for(let s of lastSelected){
                  //set up a new aura for each selected piece that has all the attributes of the aura
                  let aura = new CardAura(s.id, cardSelector, auraName);
                  for(let auraAttribute of auraAttributes){
                    //specifically don't use eventual number here because it will be evaluated in the aura update
                    aura[auraAttribute.attribute] = auraAttribute.amount;
                  }
                  queue.push(aura);
                }
              }
              break;
            }
            //RemoveBuff(pieceSelector, buffName)
            case 'RemoveBuff':
            {
              let buffName = action.args[1];
              lastSelected = this.selector.selectPieces(action.args[0], pieceSelectorParams);
              this.log.info('Remove Buff Selected %j', lastSelected);

              if(lastSelected && lastSelected.length > 0){
                for(let s of lastSelected){
                  let buff = new PieceBuff(s.id, buffName, true);
                  queue.push(buff);
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
                  && !spawnLocations.find(s => s.equals(p))
                  && !this.mapState.getTile(p).unpassable
                )
                .value();
              if(possiblePositions.length > 0){
                let position = _.sample(possiblePositions);
                spawnLocations.push(position);
                let spawn = new SpawnPiece(piece.playerId, null, action.args[0], position, null);
                queue.push(spawn);
              }else{
                this.log.info('Couldn\'t spawn piece because there\'s no where to put it');
              }

              break;
            }

            //GiveStatus(pieceSelector, Status)
            case 'GiveStatus':
            {
              lastSelected = this.selector.selectPieces(action.args[0], pieceSelectorParams);
              this.log.info('Give Status Selected %j status %s', lastSelected, action.args[1]);
              if(lastSelected && lastSelected.length > 0){
                for(let s of lastSelected){
                  queue.push(new PieceStatusChange(s.id, Statuses[action.args[1]] ));
                }
              }
              break;
            }
            //RemoveStatus(pieceSelector, Status)
            case 'RemoveStatus':
            {
              lastSelected = this.selector.selectPieces(action.args[0], pieceSelectorParams);
              this.log.info('Remove Status Selected %j status %s', lastSelected, action.args[1]);
              if(lastSelected && lastSelected.length > 0){
                for(let s of lastSelected){
                  queue.push(new PieceStatusChange(s.id, null, Statuses[action.args[1]] ));
                }
              }
              break;
            }
            //startTurnTimer(turns, repeatingInterval, ...Actions)
            //turns is a counter that gets decremented each start of the turn and fires when 0
            //so a turns value of 1 will activate on the start of the following (probably enemy) turn
            //repeating interval, if truth, determines what the turns counter is reset to every time
            //the timer activates
            case 'startTurnTimer':
            {
              let timerActions = action.args.slice(2);
              let saved = lastSelected;
              //if the start turn timer action includes a target piece Id and nothing else is saving pieces,
              //use the target for the saved pieces
              if(!saved && pieceSelectorParams.targetPieceId){
                saved = [{id: pieceSelectorParams.targetPieceId}];
              }
              let attachedPiece = piece;
              if(!attachedPiece && pieceSelectorParams.targetPieceId){
                attachedPiece = {id: pieceSelectorParams.targetPieceId};
              }
              if(timerActions && timerActions.length > 0){
                for(let timerAction of timerActions){
                  this.startTurnTimers.push({
                    saved,
                    piece: attachedPiece,
                    card,
                    playerId: pieceAction.playerId,
                    interval: action.args[1] || false,
                    timer: action.args[0],
                    timerAction: timerAction
                  });
                }
              }
              break;
            }
            //endTurnTimer(turns, repeatingInterval, ...Actions)
            //see startTurnTimer above ^
            case 'endTurnTimer':
            {
              let timerActions = action.args.slice(2);
              let saved = lastSelected;
              if(!saved && pieceSelectorParams.targetPieceId){
                saved = [{id: pieceSelectorParams.targetPieceId}];
              }
              let attachedPiece = piece;
              if(!attachedPiece && pieceSelectorParams.targetPieceId){
                attachedPiece = {id: pieceSelectorParams.targetPieceId};
              }
              if(timerActions && timerActions.length > 0){
                for(let timerAction of timerActions){
                  this.endTurnTimers.push({
                    saved,
                    piece: attachedPiece,
                    card,
                    playerId: pieceAction.playerId,
                    interval: action.args[1] || false,
                    timer: action.args[0],
                    timerAction: timerAction
                  });
                }
              }
              break;
            }
            //Move(pieceSelector, tile in areaSelector, isTeleport)
            case 'Move':
            {
              lastSelected = this.selector.selectPieces(action.args[0], pieceSelectorParams);
              if(lastSelected.length > 1){
                this.log.warn('Move selected more than one piece to move %j', lastSelected);
                break;
              }
              this.log.info('Move Selected %j', lastSelected);
              let moveTo = this.selector.selectArea(action.args[1], pieceSelectorParams);

              if(!moveTo || !moveTo.resolvedPosition){
                this.log.warn("Move didn't resolve to a position %j", moveTo);
                break;
              }

              //preemptively check for colliding piece so the action can be scrubbed early
              if(this.pieceState.pieceAt(moveTo.resolvedPosition.x, moveTo.resolvedPosition.z)){
                throw new EvalError("You can't move on top of another piece!");
              }

              if(lastSelected && lastSelected.length === 1){
                queue.push(new MovePiece(lastSelected[0].id, moveTo.resolvedPosition, true, action.args[2]));
              }
              break;
            }
            //Transform(pieceSelector, cardTemplateId, targetPieceSelector)
            //Transforms the piece selector pieces into either the cardTemplateId if it's a valid minion ID,
            //otherwise, use the targetPieceSelector to find the first minion to copy props from
            case 'Transform':
            {
              lastSelected = this.selector.selectPieces(action.args[0], pieceSelectorParams);
              this.log.info('Transform Selected %j', lastSelected);
              let cardTemplateId = action.args[1];
              let targetPieceSelector = action.args[2];

              let transformPieceId = null;
              if(targetPieceSelector){
                let transformPieces = this.selector.selectPieces(targetPieceSelector, pieceSelectorParams);
                if(transformPieces && transformPieces.length > 0){
                  transformPieceId = transformPieces[0].id;
                }
              }

              if(lastSelected && lastSelected.length > 0){
                for(let s of lastSelected){
                  queue.push(new TransformPiece(s.id, cardTemplateId, transformPieceId));
                }
              }
              break;
            }
            //GiveCard(PlayerSelector, cardTemplateId)
            case 'GiveCard':
            {
              let playerSelector = this.selector.selectPlayer(pieceAction.playerId, action.args[0]);
              let cardId = this.selector.eventualNumber(action.args[1], pieceSelectorParams);
              queue.push(new GiveCard(playerSelector, cardId));
              break;
            }
            //ShuffleToDeck(PlayerSelector, cardTemplateId)
            case 'ShuffleToDeck':
            {
              let playerSelector = this.selector.selectPlayer(pieceAction.playerId, action.args[0]);
              let cardId = this.selector.eventualNumber(action.args[1], pieceSelectorParams);
              queue.push(new ShuffleToDeck(playerSelector, cardId));
              break;
            }
            //GiveArmor(pieceSelector, amount)
            case 'GiveArmor':
            {
              lastSelected = this.selector.selectPieces(action.args[0], pieceSelectorParams);
              this.log.info('Give Armor Selected %j', lastSelected);
              if(lastSelected && lastSelected.length > 0){
                for(let s of lastSelected){
                  queue.push(new PieceArmorChange(s.id, this.selector.eventualNumber(action.args[1], pieceSelectorParams)));
                }
              }
              break;
            }
            //AttachCode(pieceSelector, eventList)
            //Second arg is like a top level event list node that will get merged into the selected pieces events
            case 'AttachCode':
            {
              lastSelected = this.selector.selectPieces(action.args[0], pieceSelectorParams);
              this.log.info('Attach Code Selected %j', lastSelected);
              if(lastSelected && lastSelected.length > 0){
                for(let s of lastSelected){
                  queue.push(new AttachCode(s.id, action.args[1]));
                }
              }
              break;
            }
            //Unsummon(pieceSelector)
            case 'Unsummon':
            {
              lastSelected = this.selector.selectPieces(action.args[0], pieceSelectorParams);
              this.log.info('Unsummon Selected %j', lastSelected);
              if(lastSelected && lastSelected.length > 0){
                for(let s of lastSelected){
                  queue.push(new UnsummonPiece(s.id));
                }
              }
              break;
            }
            //Choose(cardTemplateId1, cardTemplateId2)
            case 'Choose':
            {
              queue.push(new Choose(
                action.args[0],
                action.args[1],
                pieceAction.chooseCardTemplateId,
                {
                  playerId: pieceAction.playerId,
                  activatingPiece: pieceSelectorParams.activatingPiece,
                  selfPiece: pieceSelectorParams.selfPiece,
                  targetPieceId: pieceSelectorParams.targetPieceId,
                  position: pieceSelectorParams.position,
                  pivotPosition: pieceSelectorParams.pivotPosition
                }
              ));
              break;
            }
          }
        }
      }
    }catch(e){
      if(e instanceof EvalError){
        this.queue.push(new Message(e.message, activatingPlayerId));
        return false;
      }
      throw e;
    }

    //blast through all the items we need to add to the queue and associate the activating piece with them
    //mainly for the client
    for(let action of queue){
      action.activatingPieceId = activatingPiece ? activatingPiece.id : null;
      this.queue.push(action);
    }
    return true;
  }

  //When a piece is transformed there's some state maintenance to do
  updateTransformedPiece(piece, copiedPiece){
    //First, cleanup any timers associated with the old piece
    this.cleanupTimers(piece);

    //if we're copying an active piece, duplicate any existing timers but maintain the new piece Id
    if(copiedPiece){
      let copiedEndTurnTimers = this.endTurnTimers.filter(t => t.piece && t.piece.id === copiedPiece.id);
      let copiedStartTurnTimers = this.startTurnTimers.filter(t => t.piece && t.piece.id === copiedPiece.id);

      for(let copiedEndTurnTimer of copiedEndTurnTimers){
        let clonedTimer = _.cloneDeep(copiedEndTurnTimer);
        clonedTimer.piece = piece;
        clonedTimer.playerId = piece.playerId; //Not sure if this is really needed or might cause an issue
        this.endTurnTimers.push(clonedTimer);
      }

      for(let copiedStartTurnTimer of copiedStartTurnTimers){
        let clonedTimer = _.cloneDeep(copiedStartTurnTimer);
        clonedTimer.piece = piece;
        clonedTimer.playerId = piece.playerId; //see above
        this.startTurnTimers.push(clonedTimer);
      }
    }
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
      if(activated.interval){
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
        action: activatedTimer.timerAction,
        isTimer: true
      });
    }

    return activatedEvents;
  }

  //Whenever a piece dies that had a repeating turn timer on it we need to remove it
  cleanupTimers(killedPiece){
    let abandonedFilter = (t) => (t.interval && t.piece && t.piece.id === killedPiece.id);

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

        let targetPieceIds = this.findActionTargets(event.actions, playerId, card.isSpell);
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

      for(let arg of cardEventAction.args){

        //check for recursive step if arg is another action (for a turn timer for instance)
        if(arg.action){
          let recursivePieceIds = this.findActionTargets([arg], playerId, isSpell);
          if(recursivePieceIds){
            return recursivePieceIds;
          }
        }

        let selector = arg;

        if(arg.attributeSelector){
          selector = arg.attributeSelector;
        }

        //Selectors should always have a left
        if(!selector.left) continue;

        if(!this.selector.doesSelectorUse(selector, 'TARGET')) continue;

        let targetPieceIds = this.selector.selectPossibleTargets(
          selector, {controllingPlayerId: playerId, isSpell}
        ).map(p => p.id);

        //only allow max of 1 targetable action per event
        return targetPieceIds;
      }
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

        let areaSelector = null;
        let alsoNeedsTarget = false;
        //try to find areas in any of the actions
        for(let cardEventAction of event.actions){
          if(this.targetableActions.indexOf(cardEventAction.action) === -1) continue;

          //hacky way to see if the action needs a main target for its selection
          alsoNeedsTarget = this.selector.doesSelectorUse(cardEventAction.args[0], 'TARGET');

          for(let arg of cardEventAction.args){
            //Area selectors will always have a left
            if(!arg.left) continue;

            areaSelector = this.selector.findSelector(arg, s => s && s.area);
            if(!areaSelector) continue;

            //only allow max of 1 area action per event
            break;
          }

          //only allow max of 1 area action per event
          if(areaSelector) break;
        }

        if(areaSelector){
          let areaDescrip = this.selector.selectArea(
            areaSelector,
            {isSpell: card.isSpell, controllingPlayerId: playerId}
          );
          areas.push({
            cardId: card.id,
            event: event.event,
            areaType: areaDescrip.areaType,
            size: areaDescrip.size,
            isCursor: areaDescrip.isCursor,
            isDoubleCursor: areaDescrip.isDoubleCursor || alsoNeedsTarget,
            bothDirections: areaDescrip.bothDirections,
            selfCentered: areaDescrip.selfCentered,
            stationaryArea: areaDescrip.stationaryArea,
            centerPosition: areaDescrip.centerPosition,
            pivotPosition: areaDescrip.pivotPosition,
            areaTiles: areaDescrip.areaTiles
          });
        }
      }
    }

    return areas;
  }

  //Find cards that have a choice, and tell the client what the choices are, as well as possible targets
  //for each of the choices
  //TODO: in the future add possibility to do different areas
  //   [
  //     {cardId: 2, choices: [{ cardTemplateId: 1, targets: <return from findPossibleTargets>  } ] }
  //   ]
  findChooseCards(cards, playerId, cardDirectory){
    let choices = [];

    for(let card of cards){
      if(!card.events) continue;

      //targetable events also applies to choices
      for(let targetEvent of this.targetableEvents){
        let event = card.events.find(e => e.event === targetEvent);
        if(!event) continue;

        let choice = event.actions.find(a => a.action === 'Choose');
        if(!choice) continue;

        let chooseCardIds = [choice.args[0], choice.args[1]];
        let cardChoices = [];

        //since the directory cards won't have id's we have to find the targets individually
        //to associate them properly
        for(let chooseCardId of chooseCardIds){
          let directoryCard = cardDirectory.directory[chooseCardId];

          //   [
          //     {cardId: 2, event: 'x', targetPieceIds: [4,5,6]}
          //   ]
          let targets = this.findPossibleTargets([directoryCard], playerId);
          cardChoices.push({
            cardTemplateId: chooseCardId,
            targets: targets.length > 0 ? targets[0] : null
          });
        }

        choices.push({
          cardId: card.id,
          choices: cardChoices
        });
      }
    }

    return choices;
  }

  //Get a list of pieces with events/death events to send to the client
  findEventedPieces(){
    let eventedPieces = [];

    for(let piece of this.pieceState.pieces){
      if(!piece.events) continue;
      if(piece.isHero) continue;

      let deathEvent = piece.events.find(e => e.event === 'death');
      if(deathEvent != null){
        eventedPieces.push({pieceId: piece.id, event: 'd'});
      }

      //don't highlight pieces with the default playMinion action
      //ones with other args should be shown though
      let otherEvent = piece.events.find(e => e.event !== 'playMinion' || e.args);
      if(otherEvent != null){
        eventedPieces.push({pieceId: piece.id, event: 'e'});
      }

      //also show repeating timers
      let repeatingStartTimer = this.startTurnTimers.find(t => t.piece && t.piece.id === piece.id && t.interval);
      let repeatingEndTimer = this.endTurnTimers.find(t => t.piece && t.piece.id === piece.id && t.interval);
      if(repeatingStartTimer || repeatingEndTimer){
        eventedPieces.push({pieceId: piece.id, event: 't'});
      }
    }

    return eventedPieces;
  }

  //look through cards for any that have a condition, and if it's met
  //If a card has multiple conditions, only send if the last one is met so combo cards will highlight properly
  //   [
  //     {cardId: 2}
  //   ]
  findMetConditionCards(cards, playerId){
    let conditionals = [];

    for(let card of cards){
      if(!card.events) continue;

      for(let event of card.events){

        //try to find conditionals in any of the actions
        let conditions = event.actions.filter(a => a.condition);
        if(conditions.length == 0) continue;

        //only consider the last condition to send to the client (Combo)
        let cardEventAction = conditions[conditions.length - 1];

        let pieceSelectorParams = {
          controllingPlayerId: card.playerId,
          isSpell: card.isSpell,
          isTimer: false
        };

        let compareResult = this.selector.compareExpression(cardEventAction.condition, this.pieceState, pieceSelectorParams);
        if(compareResult.length === 0){
          continue;
        }

        conditionals.push({cardId: card.id});
      }
    }

    return conditionals;
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

  //Responsible for tracking piece damage taken across multiple "times" loops of the evaluation process
  //This is to fix a multi time hit spell overkilling a minion and wasting damage
  //Returns a bool indicating if the piece already died from previous damage taken
  checkPieceDeath(pieceDamages, pieceId){
    if(!pieceDamages[pieceId]){
      pieceDamages[pieceId] = {
        shieldRemoved: false,
        totalDamageTaken: 0
      };
    }
    let pieceStatus = pieceDamages[pieceId];
    let piece = this.pieceState.piece(pieceId);
    if(!piece) return false;

    return pieceStatus.totalDamageTaken >= piece.armor + piece.health;
  }

  //Complimentary method to above, just add to the total damage taken for pieces that did receive damage
  addPieceDamageTaken(pieceDamages, pieceId, damage){
    let pieceStatus = pieceDamages[pieceId];
    let piece = this.pieceState.piece(pieceId);
    if(!piece || !pieceStatus) return false;

    if(((piece.statuses & Statuses.Shield) == Statuses.Shield) && !pieceStatus.shieldRemoved){
      pieceStatus.shieldRemoved = true;
      return false;
    }

    //subtract the damage (since it's negative) to make a more sensible totalDamageTaken number
    pieceStatus.totalDamageTaken -= damage;

  }
}
