import test from 'tape';
import PieceHealthChange from '../game/ctac/actions/PieceHealthChange.js';
import PieceStatusChange from '../game/ctac/actions/PieceStatusChange.js';
import GamePiece from '../game/ctac/models/GamePiece.js';
import Statuses from '../game/ctac/models/Statuses.js';

export default class ProcessorServiceTests
{
  constructor(queue, cardDirectory, pieceState)
  {
    this.queue = queue;
    this.cardDirectory = cardDirectory;
    this.pieceState = pieceState;
  }

  spawnPiece(pieceState, cardTemplateId, playerId, addToState = true){
    var newPiece = pieceState.newFromCard(this.cardDirectory, cardTemplateId, playerId, null);

    if(addToState){
      pieceState.add(newPiece);
    }
    return newPiece;
  }

  async start()
  {
    test('Normal piece health change', async (t) => {

      t.plan(4);
      this.queue.init();

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
      t.equal(piece.health, beforeHealth + damage, 'Piece was damaged');

    });

    test('Shield piece health change', async (t) => {

      t.plan(7);
      this.queue.init();

      var piece = this.spawnPiece(this.pieceState, 28, 1);
      const beforeHealth = piece.health;
      const damage = -2;
      t.ok(piece.statuses & Statuses.Shield, 'Piece has Shield');

      this.queue.push(new PieceHealthChange(piece.id, damage));

      await this.queue.processUntilDone();

      const generatedActions = this.queue.iterateCompletedSince();
      let actions = [...generatedActions];
      const hpChangeAction = actions[0];
      t.equal(actions.length, 2, '2 Actions Processed');
      t.ok(hpChangeAction instanceof PieceHealthChange, 'First Action is piece health change');
      t.ok(actions[1] instanceof PieceStatusChange, 'Second Action is status change');
      t.equal(hpChangeAction.change, 0, 'Health change was blocked by shield');
      t.equal(beforeHealth, piece.health, 'Piece was not damaged');
      t.ok(!(piece.statuses & Statuses.Shield), 'Shield was removed');

    });
  }
}
