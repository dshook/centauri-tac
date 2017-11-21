using UnityEngine;
using strange.extensions.mediation.impl;
using ctac.signals;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace ctac
{
    public class TileHighlightMediator : Mediator
    {
        [Inject] public TileHighlightView view { get; set; }
        
        //[Inject] public TileHoverSignal tileHover { get; set; }

        [Inject] public MovePathFoundSignal movePathFoundSignal { get; set; }
        [Inject] public TauntTilesUpdatedSignal tauntTilesSignal { get; set; }

        [Inject] public MapModel map { get; set; }
        [Inject] public PiecesModel pieces { get; set; }
        [Inject] public GamePlayersModel players { get; set; }
        [Inject] public RaycastModel raycastModel { get; set; }
        [Inject] public PossibleActionsModel possibleActions { get; set; }

        [Inject] public IMapService mapService { get; set; }
        [Inject] public IDebugService debug { get; set; }

        private PieceModel selectedPiece = null;
        private TargetModel selectingArea = null;
        private bool isDeployingPiece = false;

        void Update()
        {
            onTileHover(raycastModel.tile);
        }

        public void onTileHover(Tile tile)
        {
            //tileHover.Dispatch(tile);
            view.onTileHover(tile);

            //Unit pathfinding highlighting
            if (selectedPiece != null && tile != null && (selectedPiece.canMove || selectedPiece.canAttack))
            {
                var gameTile = map.tiles.Get(selectedPiece.tilePosition);
                var enemyOccupyingDest = pieces.Pieces.FirstOrDefault(m => 
                        m.tilePosition == tile.position 
                        && !m.currentPlayerHasControl
                        && !FlagsHelper.IsSet(m.statuses, Statuses.Cloak)
                    );
                var path = mapService.FindMovePath(selectedPiece, enemyOccupyingDest, tile);

                if (path != null)
                {
                    movePathFoundSignal.Dispatch(new MovePathFoundModel() {
                        piece = selectedPiece,
                        startTile = gameTile,
                        tiles = path,
                        isAttack = enemyOccupyingDest != null && selectedPiece.canAttack
                    });
                }
                else
                {
                    movePathFoundSignal.Dispatch(null);
                }

                if (enemyOccupyingDest != null && path != null )
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
                movePathFoundSignal.Dispatch(null);
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
                    case AreaType.Diamond:
                        tiles = mapService.GetTilesInRadius(tile.position, selectingArea.area.size).Values.ToList();
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
                    //TODO: find out from area if it can hit friendlies
                    setAttackRangeTiles(tiles, true);
                }
            }
        }

        [ListensTo(typeof(PieceSelectedSignal))]
        public void onPieceSelected(PieceModel selectedPiece)
        {
            if(isDeployingPiece) return;

            if (selectedPiece == null)
            {
                this.selectedPiece = null;
                view.onTileSelected(null);
                //view.toggleTileFlags(null, TileHighlightStatus.Movable, true);
                setAttackRangeTiles(null);
                view.toggleTileFlags(null, TileHighlightStatus.MoveRange);
                return;
            }

            updatePieceAttackRange(selectedPiece);

            //set movement and tile selected highlights
            var gameTile = map.tiles.Get(selectedPiece.tilePosition);
            this.selectedPiece = selectedPiece;

            view.onTileSelected(gameTile);

            if (selectedPiece.canMove)
            {
                //find movement
                var moveTiles = mapService.GetMovementTilesInRadius(selectedPiece);
                //take out the central one
                moveTiles.Remove(gameTile.position);
                view.toggleTileFlags(moveTiles.Values.ToList(), TileHighlightStatus.MoveRange);
            }
        }

        [ListensTo(typeof(PieceSpawningSignal))]
        public void onPieceSpawning(CardSelectedModel cardModel)
        {
            if (cardModel != null)
            {
                //find play radius depending on the card to show spawning area for a piece
                var playerHero = pieces.Hero(cardModel.card.playerId);
                var kingTiles = mapService.GetKingTilesInRadius(playerHero.tilePosition, 1);
                var heroTile = mapService.Tile(playerHero.tilePosition);
                var piecePositions = pieces.Pieces.Select(p => p.tilePosition);
                List<Tile> playableTiles = kingTiles.Values.ToList()
                    .Where(t => 
                        !piecePositions.Contains(t.position)
                        && mapService.isHeightPassable(t, heroTile)
                        && !t.unpassable
                    )
                    .ToList();
                view.toggleTileFlags(playableTiles, TileHighlightStatus.Selected, true);
                isDeployingPiece = true;
            }
            else
            {
                pieceNotDeploying();
            }
        }

        [ListensTo(typeof(PieceDiedSignal))]
        public void onPieceDied(PieceModel piece)
        {
            if (selectedPiece != null && piece.id == selectedPiece.id)
            {
                selectedPiece = null;
            }
            updateTauntTiles(piece);
            view.onTileSelected(null);
            view.toggleTileFlags(null, TileHighlightStatus.Movable);
            setAttackRangeTiles(null);
            onPieceHover(null);
        }

        private PieceModel hoveredPiece = null;
        [ListensTo(typeof(PieceHoverSignal))]
        public void onPieceHover(PieceModel piece)
        {
            hoveredPiece = piece;
            //whenever we get a null piece hover we'll clear right away, but with a real piece, put in a delay before running logic
            if (piece == null)
            {
                onRealPieceHover(piece);
            }
            else
            {
                StartCoroutine(WaitAndCall(0.3f));
            }
        }

        IEnumerator WaitAndCall(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            onRealPieceHover(hoveredPiece);
        }

        private void onRealPieceHover(PieceModel piece)
        {
            if(isDeployingPiece) return;

            if (piece != null 
                && selectedPiece == null 
                && piece.canMove //might be an issue with checking hasMoved inside of canMove
                )
            {
                updatePieceAttackRange(piece);
            }
            else
            {
                if (selectedPiece == null)
                {
                    view.toggleTileFlags(null, TileHighlightStatus.MoveRange, true);
                    setAttackRangeTiles(null);
                }
            }

            //attacking enemy at range
            if (
                selectedPiece != null
                && selectedPiece.attackCount < selectedPiece.maxAttacks
                && piece != null
                && piece != selectedPiece
                && selectedPiece.isRanged
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

        private void updatePieceAttackRange(PieceModel piece)
        {
            //check for ranged units first since they can't move and attack
            if (!piece.canAttack)
            {
                setAttackRangeTiles(null, false);
            }
            else if (piece.isRanged)
            {
                var attackRangeTiles = mapService.GetKingTilesInRadius(piece.tilePosition, piece.range.Value);
                setAttackRangeTiles(attackRangeTiles.Values.ToList(), !piece.currentPlayerHasControl);
            }
            else if (piece.isMelee)
            {
                //melee units

                var movePositions = mapService.GetMovementTilesInRadius(piece);
                var moveTiles = movePositions.Values.ToList();

                List<Tile> attackTiles = null;
                if (piece.canAttack)
                {
                    var attackPositions = mapService.Expand(movePositions.Keys.ToList(), 1);
                    attackTiles = attackPositions.Values.ToList();

                    //find diff to get just attack tiles
                    //also take friendly units and untargetable enemies like Cloak
                    attackTiles = attackTiles
                        .Except(moveTiles)
                        .Where(t => {
                            var occupyingPiece = pieces.Pieces.FirstOrDefault(m => m.tilePosition == t.position);
                            return !t.unpassable
                                && (
                                    occupyingPiece == null
                                    || (
                                        occupyingPiece.playerId != piece.playerId 
                                        && !FlagsHelper.IsSet(occupyingPiece.statuses, Statuses.Cloak)
                                        )
                                );
                        })
                        .ToList();
                }

                //take out the central one
                var center = moveTiles.FirstOrDefault(t => t.position == piece.tilePosition);
                moveTiles.Remove(center);

                view.toggleTileFlags(moveTiles, TileHighlightStatus.MoveRange, true);
                setAttackRangeTiles(attackTiles, !piece.currentPlayerHasControl);
            }
        }

        [ListensTo(typeof(PieceFinishedMovingSignal))]
        public void onPieceMove(PieceModel piece)
        {
            updateTauntTiles();
        }

        [ListensTo(typeof(PieceSpawnedSignal))]
        public void onPieceSpawn(PieceSpawnedModel piece)
        {
            updateTauntTiles();
            pieceNotDeploying();
        }

        [ListensTo(typeof(ActionCancelledSpawnPieceSignal))]
        public void onPieceSpawnCancelled(SpawnPieceModel piece, SocketKey key)
        {
            onPieceSpawn(null);
        }

        [ListensTo(typeof(TurnEndedSignal))]
        public void onTurnEnded(GameTurnModel gameTurn)
        {
            updateTauntTiles();
        }

        [ListensTo(typeof(PieceStatusChangeSignal))]
        public void onPieceStatusChange(PieceStatusChangeModel change)
        {
            updateTauntTiles();
        }

        private void pieceNotDeploying()
        {
            //remove piece deploying highlights
            view.toggleTileFlags(null, TileHighlightStatus.Selected, true);
            isDeployingPiece = false;
        }

        private void updateTauntTiles(PieceModel deadPiece = null)
        {
            var tauntPieces = pieces.Pieces.Where(p => FlagsHelper.IsSet(p.statuses, Statuses.Taunt)
                && (deadPiece == null || p != deadPiece));
            var friendlyTauntTiles = new List<Tile>();
            var friendlyTauntPieceTiles = new List<Tile>();
            var enemyTauntTiles = new List<Tile>();
            var enemyTauntPieceTiles = new List<Tile>();
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
                    friendlyTauntPieceTiles.Add(mapService.Tile(tauntPiece.tilePosition));
                }
                else
                {
                    enemyTauntTiles.AddRange(kingTiles.Values);
                    enemyTauntPieceTiles.Add(mapService.Tile(tauntPiece.tilePosition));
                }
            }
            friendlyTauntTiles = friendlyTauntTiles.Distinct().ToList();
            enemyTauntTiles = enemyTauntTiles.Distinct().ToList();

            tauntTilesSignal.Dispatch(new TauntTilesUpdateModel()
            {
                friendlyTauntTiles = friendlyTauntTiles,
                friendlyTauntPieceTiles = friendlyTauntPieceTiles,
                enemyTauntTiles = enemyTauntTiles,
                enemyTauntPieceTiles = enemyTauntPieceTiles,
            });
        }

        [ListensTo(typeof(StartSelectTargetSignal))]
        public void onStartTarget(TargetModel model)
        {
            //see if there are any areas to show
            var area = model.area;
            if (model.area != null)
            {
                selectingArea = model;
                if(area.areaTiles.Count > 0){
                    var tiles = map.getTilesByPosition(area.areaTiles.Select(t => t.Vector2).ToList());
                    setAttackRangeTiles(tiles, true);
                }
            }
            updateSelectHighlights(model);
        }

        [ListensTo(typeof(UpdateTargetSignal))]
        public void onUpdateTarget(TargetModel model)
        {
            updateSelectHighlights(model);
        }

        [ListensTo(typeof(CancelSelectTargetSignal))]
        public void onCancelSelectTarget(CardModel card)
        {
            setAttackRangeTiles(null);
            selectingArea = null;
            updateSelectHighlights(null);
            pieceNotDeploying();
        }

        [ListensTo(typeof(SelectTargetSignal))]
        public void onSelectTarget(TargetModel card)
        {
            setAttackRangeTiles(null);
            selectingArea = null;
            updateSelectHighlights(null);
        }

        private void updateSelectHighlights(TargetModel model)
        {
            view.toggleTileFlags(null, TileHighlightStatus.TargetTile, true);
            if (model != null && model.area != null)
            {
                if (model.selectedPosition.HasValue)
                {
                    List<Tile> tiles = null;
                    switch (model.area.areaType)
                    {
                        case AreaType.Square:
                            tiles = mapService.GetKingTilesInRadius(model.selectedPosition.Value, selectingArea.area.size).Values.ToList();
                            break;
                        case AreaType.Diamond:
                            tiles = mapService.GetTilesInRadius(model.selectedPosition.Value, selectingArea.area.size).Values.ToList();
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
                    view.toggleTileFlags(tiles, TileHighlightStatus.TargetTile, true);
                }
                else if (model.targets != null && model.targets.targetPieceIds.Count > 0)
                {
                }
                else
                {
                    view.toggleTileFlags(map.tileList, TileHighlightStatus.TargetTile, true);
                }
            }
        }

        //Special method for attack range so we can also set the piece indicator view up properly
        private void setAttackRangeTiles(List<Tile> tiles, bool highlightFriendly = true)
        {
            foreach (var tile in map.tileList)
            {
                tile.pieceIndicatorView.SetStatus(false);
            }
            if (tiles != null)
            {
                foreach (var tile in tiles)
                {
                    var occupying = pieces.PieceAt(tile.position);
                    if(occupying != null && (highlightFriendly || !occupying.currentPlayerHasControl))
                    {
                        tile.pieceIndicatorView.SetStatus(true, !occupying.currentPlayerHasControl);
                    }

                }
            }
            view.toggleTileFlags(tiles, TileHighlightStatus.AttackRange);
        }
    }
}

