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

        [Inject] public StartSelectTargetSignal startSelectTarget { get; set; }
        [Inject] public SelectTargetSignal selectTarget { get; set; }
        [Inject] public UpdateTargetSignal updateTarget { get; set; }
        [Inject] public CancelSelectTargetSignal cancelSelectTarget { get; set; }

        [Inject] public MapModel map { get; set; }
        [Inject] public PiecesModel pieces { get; set; }
        [Inject] public GameTurnModel gameTurn { get; set; }
        [Inject] public RaycastModel raycastModel { get; set; }
        [Inject] public PossibleActionsModel possibleActions { get; set; }

        [Inject] public IMapService mapService { get; set; }

        private PieceModel selectedPiece = null;
        private TargetModel selectingArea = null;

        public override void OnRegister()
        {
            cardSelected.AddListener(onCardSelected);
            pieceSelected.AddListener(onPieceSelected);
            pieceDied.AddListener(onPieceDied);
            pieceHoveredSignal.AddListener(onPieceHover);
            pieceMoved.AddListener(onPieceMove);
            pieceSpawned.AddListener(onPieceSpawn);
            pieceStatusChanged.AddListener(onPieceStatusChange);
            turnEnded.AddListener(onTurnEnded);

            cancelSelectTarget.AddListener(onCancelSelectTarget);
            updateTarget.AddListener(onUpdateTarget);
            selectTarget.AddListener(onSelectTarget);
            startSelectTarget.AddListener(onStartTarget);
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

            cancelSelectTarget.RemoveListener(onCancelSelectTarget);
            updateTarget.AddListener(onUpdateTarget);
            selectTarget.RemoveListener(onSelectTarget);
            startSelectTarget.RemoveListener(onStartTarget);
        }

        void Update()
        {
            onTileHover(raycastModel.tile);
        }

        void onTileHover(Tile tile)
        {
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

                //don't show move path for ranged units hovering over an enemy
                if (!(enemyOccupyingDest && selectedPiece.range.HasValue))
                {
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
                }
            }
            else
            {
                view.toggleTileFlags(null, TileHighlightStatus.PathFind);
                view.onAttackTile(null);
            }

            //display area preview on area targeting
            if (
                selectingArea != null 
                && selectingArea.area != null 
                && selectingArea.area.isCursor 
                && !selectingArea.area.stationaryArea
                && tile != null
                && FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.TargetTile)
                )
            {
                List<Tile> tiles = null;
                switch (selectingArea.area.areaType) {
                    case AreaType.Square:
                        tiles = mapService.GetKingTilesInRadius(tile.position, selectingArea.area.size).Values.ToList();
                        break;
                    case AreaType.Cross:
                        tiles = mapService.GetCrossTiles(tile.position, selectingArea.area.size).Values.ToList();
                        break;
                    case AreaType.Line:
                    case AreaType.Row:
                    case AreaType.Diagonal:
                        if (selectingArea.selectedPosition != null)
                        {
                            tiles = mapService.GetLineTiles(
                                selectingArea.selectedPosition.Value, 
                                tile.position, 
                                selectingArea.area.size,
                                selectingArea.area.bothDirections ?? false
                             ).Values.ToList();
                        }
                        break;
                }
                if (tiles != null)
                {
                    view.toggleTileFlags(tiles, TileHighlightStatus.AttackRange);
                }
            }
        }

        private void onPieceSelected(PieceModel selectedPiece)
        {
            if (selectedPiece == null)
            {
                this.selectedPiece = null;
                view.onTileSelected(null);
                view.toggleTileFlags(null, TileHighlightStatus.Movable);
                view.toggleTileFlags(null, TileHighlightStatus.AttackRange);
                return;
            }

            //check for attack range tiles
            if (selectedPiece.range.HasValue)
            {
                var attackTiles = mapService.GetTilesInRadius(selectedPiece.tilePosition, selectedPiece.range.Value);
                view.toggleTileFlags(attackTiles.Values.ToList(), TileHighlightStatus.AttackRange);
            }

            //set movement and tile selected highlights
            if ( FlagsHelper.IsSet(selectedPiece.statuses, Statuses.Paralyze)
                || FlagsHelper.IsSet(selectedPiece.statuses, Statuses.Root)
                || selectedPiece.movement <= 0
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

        private void onCardSelected(CardSelectedModel cardModel)
        {
            if (cardModel == null)
            {
                view.toggleTileFlags(null, TileHighlightStatus.Selected);
                return;
            }

            if (!cardModel.card.isSpell)
            {

                //find play radius depending on the card
                var playerHero = pieces.Hero(cardModel.card.playerId);
                List<Tile> playableTiles = map.tileList
                    .Where(t => mapService.KingDistance(playerHero.tilePosition, t.position) == 1
                        && !pieces.Pieces.Select(p => p.tilePosition).Contains(t.position)
                        && mapService.isHeightPassable(t, mapService.Tile(playerHero.tilePosition))
                    )
                    .ToList();
                view.toggleTileFlags(playableTiles, TileHighlightStatus.Selected);
            }
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
                if (piece.attack <= 0)
                {
                    view.toggleTileFlags(null, TileHighlightStatus.AttackRange);
                }
                //check for ranged units first since they can't move and attack
                else if (piece.range.HasValue && piece.attack > 0)
                {
                    var attackRangeTiles = mapService.GetTilesInRadius(piece.tilePosition, piece.range.Value);
                    view.toggleTileFlags(attackRangeTiles.Values.ToList(), TileHighlightStatus.AttackRange);
                }
                else
                {
                    //melee units

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
                    view.toggleTileFlags(attackTiles, TileHighlightStatus.AttackRange);
                }
            }
            else
            {
                view.toggleTileFlags(null, TileHighlightStatus.MoveRange);
                if (selectedPiece == null)
                {
                    view.toggleTileFlags(null, TileHighlightStatus.AttackRange);
                }
            }

            //attacking enemy at range
            if (
                selectedPiece != null
                && selectedPiece.attackCount < selectedPiece.maxAttacks
                && piece != null
                && piece != selectedPiece
                && selectedPiece.range.HasValue
                )
            {
                if (mapService.TileDistance(selectedPiece.tilePosition, piece.tilePosition)
                    <= selectedPiece.range.Value)
                {
                    var gameTile = map.tiles.Get(piece.tilePosition);
                    view.onAttackTile(gameTile);
                }
                else
                {
                    view.onAttackTile(null);
                }
            }
            else
            {
                view.onAttackTile(null);
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

        private void onStartTarget(TargetModel model)
        {
            //see if there are any areas to show
            var area = model.area;
            if (model.area != null)
            {
                selectingArea = model;
                if(area.areaTiles.Count > 0){
                    var tiles = map.getTilesByPosition(area.areaTiles.Select(t => t.Vector2).ToList());
                    view.toggleTileFlags(tiles, TileHighlightStatus.AttackRange);
                }
            }
            updateSelectHighlights(model);
        }

        private void onUpdateTarget(TargetModel model)
        {
            updateSelectHighlights(model);
        }

        private void onCancelSelectTarget(CardModel card)
        {
            view.toggleTileFlags(null, TileHighlightStatus.AttackRange);
            selectingArea = null;
            updateSelectHighlights(null);
        }

        private void onSelectTarget(TargetModel card)
        {
            view.toggleTileFlags(null, TileHighlightStatus.AttackRange);
            selectingArea = null;
            updateSelectHighlights(null);
        }

        private void updateSelectHighlights(TargetModel model)
        {
            view.toggleTileFlags(null, TileHighlightStatus.TargetTile);
            if (model != null && model.area != null)
            {
                if (model.selectedPosition.HasValue)
                {
                    List<Tile> tiles = null;
                    switch (model.area.areaType) {
                        case AreaType.Square:
                            tiles = mapService.GetKingTilesInRadius(model.selectedPosition.Value, selectingArea.area.size).Values.ToList();
                            break;
                        case AreaType.Cross:
                            tiles = mapService.GetCrossTiles(model.selectedPosition.Value, selectingArea.area.size).Values.ToList();
                            break;
                        case AreaType.Line:
                            tiles = mapService.GetKingTilesInRadius(model.selectedPosition.Value, 1).Values.ToList();
                            break;
                        case AreaType.Row:
                            tiles = mapService.GetCrossTiles(model.selectedPosition.Value, 1).Values.ToList();
                            break;
                        case AreaType.Diagonal:
                            tiles = mapService.GetDiagonalTilesInRadius(model.selectedPosition.Value, 1).Values.ToList();
                            break;
                    }
                    view.toggleTileFlags(tiles, TileHighlightStatus.TargetTile);
                }
                else
                {
                    view.toggleTileFlags(map.tileList, TileHighlightStatus.TargetTile);
                }
            }
        }
    }
}

