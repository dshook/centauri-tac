using UnityEngine;
using strange.extensions.mediation.impl;
using ctac.signals;
using System.Linq;
using System.Collections.Generic;

namespace ctac
{
    public class TileHighlightMediator : Mediator
    {
        [Inject] public TileHighlightView view { get; set; }
        
        [Inject] public TileHoverSignal tileHover { get; set; }

        [Inject] public CardSelectedSignal cardSelected { get; set; }

        [Inject] public PieceSelectedSignal pieceSelected { get; set; }
        [Inject] public PieceHoverSignal pieceHoveredSignal { get; set; }

        [Inject] public PieceDiedSignal pieceDied { get; set; }
        [Inject] public PieceFinishedMovingSignal pieceMoved { get; set; }
        [Inject] public PieceSpawnedSignal pieceSpawned { get; set; }
        [Inject] public PieceStatusChangeSignal pieceStatusChanged { get; set; }
        [Inject] public TurnEndedSignal turnEnded { get; set; }

        [Inject] public MapModel map { get; set; }
        [Inject] public PiecesModel pieces { get; set; }
        [Inject] public GameTurnModel gameTurn { get; set; }

        [Inject] public IMapService mapService { get; set; }

        private PieceModel selectedPiece = null;

        public override void OnRegister()
        {
            view.tileHover.AddListener(onTileHover);
            cardSelected.AddListener(onCardSelected);
            pieceSelected.AddListener(onPieceSelected);
            pieceDied.AddListener(onPieceDied);
            pieceHoveredSignal.AddListener(onPieceHover);
            pieceMoved.AddListener(onPieceMove);
            pieceSpawned.AddListener(onPieceSpawn);
            pieceStatusChanged.AddListener(onPieceStatusChange);
            turnEnded.AddListener(onTurnEnded);
            view.init();
        }

        public override void onRemove()
        {
            pieceSelected.RemoveListener(onPieceSelected);
            cardSelected.RemoveListener(onCardSelected);
            pieceHoveredSignal.RemoveListener(onPieceHover);
            pieceDied.RemoveListener(onPieceDied);
            pieceMoved.RemoveListener(onPieceMove);
            pieceSpawned.RemoveListener(onPieceSpawn);
            pieceStatusChanged.RemoveListener(onPieceStatusChange);
            turnEnded.RemoveListener(onTurnEnded);
        }

        void onTileHover(GameObject newHoverTile)
        {
            Tile tile = null;
            if (newHoverTile != null)
            {
                tile = map.tiles.Get(newHoverTile.transform.position.ToTileCoordinates());
            }
            tileHover.Dispatch(tile);
            view.onTileHover(tile);

            if (selectedPiece != null && tile != null && !selectedPiece.hasMoved)
            {
                var gameTile = map.tiles.Get(selectedPiece.tilePosition);
                var enemyOccupyingDest = pieces.Pieces.Any(m => 
                        m.tilePosition == tile.position 
                        && !m.currentPlayerHasControl
                        && !FlagsHelper.IsSet(m.statuses, Statuses.Cloak)
                    );
                //add an extra tile of movement if the destination is an enemy to attack since you don't have to go all the way to them
                var boost = enemyOccupyingDest ? 1 : 0;
                var path = mapService.FindPath(gameTile, tile, selectedPiece.movement + boost, gameTurn.currentPlayerId);
                view.toggleTileFlags(path, TileHighlightStatus.PathFind);

                if (enemyOccupyingDest
                    && path != null
                )
                {
                    view.onAttackTile(tile);
                }
                else
                {
                    view.onAttackTile(null);
                }
            }
            else
            {
                view.toggleTileFlags(null, TileHighlightStatus.PathFind);
                view.onAttackTile(null);
            }
        }

        private void onPieceSelected(PieceModel selectedPiece)
        {
            if (selectedPiece != null && !selectedPiece.isMoving)
            {
                if ( FlagsHelper.IsSet(selectedPiece.statuses, Statuses.Paralyze)
                    || FlagsHelper.IsSet(selectedPiece.statuses, Statuses.Root)
                ) {
                    return;
                }
                var gameTile = map.tiles.Get(selectedPiece.tilePosition);
                this.selectedPiece = selectedPiece;

                view.onTileSelected(gameTile);

                if (!selectedPiece.hasMoved)
                {
                    //find movement
                    var moveTiles = mapService.GetMovementTilesInRadius(gameTile.position, selectedPiece.movement, selectedPiece.playerId);
                    //take out the central one
                    moveTiles.Remove(gameTile.position);
                    view.toggleTileFlags(moveTiles.Values.ToList(), TileHighlightStatus.Movable);
                }
            }
            else
            {
                this.selectedPiece = null;
                view.onTileSelected(null);
                view.toggleTileFlags(null, TileHighlightStatus.Movable);
            }
        }

        private void onCardSelected(CardModel card)
        {
            if (card == null)
            {
                view.toggleTileFlags(null, TileHighlightStatus.Selected);
                return;
            }

            if(card.tags.Contains("Spell")) return;

            //find play radius depending on the card
            var playerHero = pieces.Hero(card.playerId);
            List<Tile> playableTiles = map.tileList
                .Where(t => mapService.KingDistance(playerHero.tilePosition, t.position) == 1
                    && !pieces.Pieces.Select(p => p.tilePosition).Contains(t.position)
                )
                .ToList();
            view.toggleTileFlags(playableTiles, TileHighlightStatus.Selected);
        }

        private void onPieceDied(PieceModel piece)
        {
            if (selectedPiece != null && piece.id == selectedPiece.id)
            {
                selectedPiece = null;
            }
            updateTauntTiles(piece);
            view.onTileSelected(null);
            view.toggleTileFlags(null, TileHighlightStatus.Movable);
            onPieceHover(null);
        }

        private void onPieceHover(PieceModel piece)
        {
            if (piece != null 
                && selectedPiece == null 
                && !FlagsHelper.IsSet(piece.statuses, Statuses.Paralyze)
                && !FlagsHelper.IsSet(piece.statuses, Statuses.Root)
                )
            {
                var movePositions = mapService.GetMovementTilesInRadius(piece.tilePosition, piece.movement, piece.playerId);
                var moveTiles = movePositions.Values.ToList();
                var attackPositions = mapService.Expand(movePositions.Keys.ToList(), 1);
                var attackTiles = attackPositions.Values.ToList();

                //find diff to get just attack tiles
                attackTiles = attackTiles.Except(moveTiles).ToList();

                //take out the central one
                var center = moveTiles.FirstOrDefault(t => t.position == piece.tilePosition);
                moveTiles.Remove(center);

                //TODO: take friendly units out of move and untargetable enemies like Cloak

                view.toggleTileFlags(moveTiles, TileHighlightStatus.MoveRange);
                if (piece.attack > 0)
                {
                    view.toggleTileFlags(attackTiles, TileHighlightStatus.AttackRange);
                }
                else
                {
                    view.toggleTileFlags(null, TileHighlightStatus.AttackRange);
                }
            }
            else
            {
                view.toggleTileFlags(null, TileHighlightStatus.MoveRange);
                view.toggleTileFlags(null, TileHighlightStatus.AttackRange);
            }
        }

        private void onPieceMove(PieceModel piece)
        {
            updateTauntTiles();
        }

        private void onPieceSpawn(PieceModel piece)
        {
            updateTauntTiles();
        }

        private void onTurnEnded(GameTurnModel gameTurn)
        {
            updateTauntTiles();
        }

        private void onPieceStatusChange(PieceStatusChangeModel change)
        {
            updateTauntTiles();
        }

        private void updateTauntTiles(PieceModel deadPiece = null)
        {
            var tauntPieces = pieces.Pieces.Where(p => FlagsHelper.IsSet(p.statuses, Statuses.Taunt)
                && (deadPiece == null || p != deadPiece));
            var friendlyTauntTiles = new List<Tile>();
            var enemyTauntTiles = new List<Tile>();
            foreach (var tauntPiece in tauntPieces)
            {
                var kingTiles = mapService.GetKingTilesInRadius(tauntPiece.tilePosition, 1);
                //filter tiles that are too high/low to protect
                kingTiles = kingTiles.Where(t => 
                    mapService.isHeightPassable(t.Value, mapService.Tile(tauntPiece.tilePosition))
                ).ToDictionary(k => k.Key, v => v.Value);


                if (tauntPiece.currentPlayerHasControl)
                {
                    friendlyTauntTiles.AddRange(kingTiles.Values);
                }
                else
                {
                    enemyTauntTiles.AddRange(kingTiles.Values);
                }
            }

            if (friendlyTauntTiles.Count > 0)
            {
                view.toggleTileFlags(friendlyTauntTiles.Distinct().ToList(), TileHighlightStatus.FriendlyTauntArea);
            }
            else
            {
                view.toggleTileFlags(null, TileHighlightStatus.FriendlyTauntArea);
            }

            if (enemyTauntTiles.Count > 0)
            {
                view.toggleTileFlags(enemyTauntTiles.Distinct().ToList(), TileHighlightStatus.EnemyTauntArea);
            }
            else
            {
                view.toggleTileFlags(null, TileHighlightStatus.EnemyTauntArea);
            }
        }
    }
}

