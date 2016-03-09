import test from 'tape';
import PieceHealthChange from '../game/ctac/actions/PieceHealthChange.js';
import GamePiece from '../game/ctac/models/GamePiece.js';

export default class ProcessorServiceTests
{
  constructor(queue, cardDirectory, pieceState)
  {
    this.queue = queue;
    this.cardDirectory = cardDirectory;
    this.pieceState = pieceState;
  }

  spawnPiece(pieceState, cardTemplateId, playerId, addToState = true){
      let cardPlayed = this.cardDirectory.directory[cardTemplateId];

      var newPiece = new GamePiece();
      newPiece.playerId = playerId;
      newPiece.cardTemplateId = cardTemplateId;
      newPiece.attack = cardPlayed.attack;
      newPiece.health = cardPlayed.health;
      newPiece.baseAttack = cardPlayed.attack;
      newPiece.baseHealth = cardPlayed.health;
      newPiece.movement = cardPlayed.movement;
      newPiece.baseMovement = cardPlayed.movement;
      newPiece.tags = cardPlayed.tags;
      newPiece.statuses = cardPlayed.statuses;

      if(addToState){
        pieceState.add(newPiece);
      }
      return newPiece;
  }

  async start()
  {
    test('Normal piece health change', async (t) => {

      t.plan(4);

      var piece = this.spawnPiece(this.pieceState, 1, 1);
      const beforeHealth = piece.health;
      const damage = -2;

      this.queue.push(new PieceHealthChange(piece.id, damage));

      await this.queue.processUntilDone();

      const generatedActions = this.queue.iterateCompletedSince();
      let actions = [...generatedActions];

      const hpChangeAction = actions[0];
      t.equal(actions.length, 1, '1 Actions Processed');
      t.ok(hpChangeAction instanceof PieceHealthChange, 'First Action is piece health change');
      t.equal(hpChangeAction.change, damage, 'Action change is equal to damage');
      t.equal(beforeHealth + damage, piece.health, 'Piece was damaged');

    });
  }
}
