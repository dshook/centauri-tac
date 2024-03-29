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
        [Inject] public CursorMessageSignal cursorMessageSignal { get; set; }
        [Inject] public CursorSignal cursorSignal { get; set; }

        [Inject] public MapModel map { get; set; }
        [Inject] public PiecesModel pieces { get; set; }
        [Inject] public GamePlayersModel players { get; set; }
        [Inject] public RaycastModel raycastModel { get; set; }
        [Inject] public PossibleActionsModel possibleActions { get; set; }

        [Inject] public IMapService mapService { get; set; }
        [Inject] public IDebugService debug { get; set; }

        private PieceModel selectedPiece = null;
        private PieceModel hoveredPiece = null;
        private TargetModel selectingArea = null;
        private MovePathFoundModel movePath = null;
        private bool isDeployingPiece = false;

        void Update()
        {
            if(raycastModel.cardCanvasHit == null){
                onTileHover(raycastModel.tile, raycastModel.piece);
            }
        }

        public void onTileHover(Tile tile, PieceView piece)
        {
            if(piece != null){
                tile = map.tiles[piece.piece.tilePosition];
            }
            //tileHover.Dispatch(tile);
            view.onTileHover(tile);

            //Unit pathfinding highlighting
            if (
                selectedPiece != null
                && selectedPiece.currentPlayerHasControl
                && tile != null
                && (selectedPiece.canMove || selectedPiece.canAttack)
            )
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
                    if(enemyOccupyingDest == null){ cursorSignal.Dispatch(CursorStyles.Walking); }
                }
                else
                {
                    movePathFoundSignal.Dispatch(null);
                    cursorSignal.Dispatch(CursorStyles.Default);
                }

            }
            else
            {
                view.toggleTileFlags(null, TileHighlightStatus.PathFind);
                movePathFoundSignal.Dispatch(null);
                cursorSignal.Dispatch(CursorStyles.Default);
            }

            //display area preview on area targeting
            if (
                selectingArea != null
                && selectingArea.cardArea != null
                && selectingArea.cardArea.isCursor
                && !selectingArea.cardArea.stationaryArea
                && tile != null
                && FlagsHelper.IsSet(tile.highlightStatus, TileHighlightStatus.TargetTile)
                )
            {
                List<Tile> tiles = GetAreaTiles(selectingArea.cardArea, tile.position);
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
                view.toggleTileFlags(null, TileHighlightStatus.AttackRangeTotal);
                updatePieceMoveRange(null);
                return;
            }

            updatePieceAttackRange(selectedPiece);

            //set movement and tile selected highlights
            var gameTile = map.tiles.Get(selectedPiece.tilePosition);
            this.selectedPiece = selectedPiece;

            view.onTileSelected(gameTile);

            updatePieceMoveRange(selectedPiece);
        }

        [ListensTo(typeof(PieceSpawningSignal))]
        public void onPieceSpawning(CardSelectedModel cardModel)
        {
            if (cardModel != null)
            {
                //find play radius depending on the card to show spawning area for a piece
                var playerHero = pieces.Hero(cardModel.card.playerId);
                var isAirdrop = (cardModel.card.statuses & Statuses.Airdrop) != 0;
                var allowableDistance = isAirdrop ? 4 : 1;
                var kingTiles = mapService.GetKingTilesInRadius(playerHero.tilePosition, allowableDistance);
                var heroTile = mapService.Tile(playerHero.tilePosition);
                var piecePositions = pieces.Pieces.Select(p => p.tilePosition);
                List<Tile> playableTiles = kingTiles.Values.ToList()
                    .Where(t =>
                        !piecePositions.Contains(t.position)
                        && (isAirdrop || mapService.isHeightPassable(t, heroTile))
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
            setAttackRangeTiles(null);
            view.toggleTileFlags(null, TileHighlightStatus.AttackRangeTotal);
            onPieceHover(null);
        }

        [ListensTo(typeof(PieceHoverSignal))]
        public void onPieceHover(PieceModel piece)
        {
            if(hoveredPiece != null){
                //Probably should go somewhere else but I want it to be synced up with the tile highlight delay
                hoveredPiece.pieceView.hovered = false;
            }
            if(piece != null){
                piece.pieceView.hovered = true;
            }

            hoveredPiece = piece;
            //whenever we get a null piece hover we'll clear right away, but with a real piece, put in a delay before running logic
            if (piece == null)
            {
                onRealPieceHover(piece);
            }
            else
            {
                StartCoroutine(WaitAndCall(0.2f));
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

            Tile gameTile = null;
            string cursorMessage = null;
            if(piece != null){
                gameTile = map.tiles.Get(piece.tilePosition);

                var pieceAreas = possibleActions.GetAreasForPiece(piece.playerId, piece.id);
                if(pieceAreas != null){
                    List<Tile> tiles = GetAreaTiles(pieceAreas, piece.tilePosition);
                    if (tiles != null)
                    {
                        //TODO: find out from area if it can hit friendlies
                        setAttackRangeTiles(tiles, true);
                    }
                }
            }

            if (piece != null && selectedPiece == null)
            {
                updatePieceAttackRange(piece);
                updatePieceMoveRange(piece);
            }
            else if(piece == null && selectedPiece == null)
            {
                updatePieceAttackRange(null);
                updatePieceMoveRange(null);
            }

            //attacking enemy
            if (
                selectedPiece != null
                && selectedPiece.currentPlayerHasControl
                && selectedPiece.attackCount < selectedPiece.maxAttacks
                && piece != null
                && piece != selectedPiece
                && piece.playerId != selectedPiece.playerId
                )
            {
                // var tileDistance = mapService.TileDistance(selectedPiece.tilePosition, piece.tilePosition);
                var kingDistance = mapService.KingDistance(selectedPiece.tilePosition, piece.tilePosition);

                if (selectedPiece.isRanged && kingDistance <= selectedPiece.range.Value)
                {
                    onAttackTile( new List<Tile>(){gameTile});

                    var attackingFromTile = map.tiles.Get(selectedPiece.tilePosition);
                    //b-b-b-bonus
                    if(attackingFromTile.fullPosition.y - gameTile.fullPosition.y >= Constants.heightDeltaThreshold)
                    {
                        cursorMessage = "+1 High Ground";
                    }
                    //s-s-s-suck it
                    if(gameTile.fullPosition.y - attackingFromTile.fullPosition.y  >= Constants.heightDeltaThreshold)
                    {
                        cursorMessage = "-1 Low Ground";
                    }
                }
                else if(selectedPiece.isMelee && movePath != null)
                {
                    var attackTiles = new List<Tile>(){gameTile};
                    //work out what direction the piece will be facing if they walk over and attack
                    var endPosition = movePath.tiles.Count > 1 ? movePath.tiles[movePath.tiles.Count - 2].position : selectedPiece.tilePosition;
                    var endDirection = mapService.FaceDirection(
                        endPosition,
                        movePath.tiles[movePath.tiles.Count - 1].position
                    );
                    if ((selectedPiece.statuses & Statuses.Cleave) != 0)
                    {
                        attackTiles.AddRange(mapService.CleavePositions(endPosition, endDirection));
                    }
                    if ((selectedPiece.statuses & Statuses.Piercing) != 0)
                    {
                        attackTiles.AddRange(mapService.PiercePositions(endPosition, endDirection));
                    }
                    onAttackTile(attackTiles);
                }
                else
                {
                    onAttackTile(null);
                }
            }
            else
            {
                onAttackTile(null);
            }

            if(!string.IsNullOrEmpty(cursorMessage)){
                cursorMessageSignal.Dispatch(new MessageModel(){message = cursorMessage});
            }else{
                cursorMessageSignal.Dispatch(null);
            }
        }

        void onAttackTile(List<Tile> newTiles){
            cursorSignal.Dispatch(newTiles != null ? CursorStyles.Attack : CursorStyles.Default);
            view.onAttackTile(newTiles);
        }

        private void updatePieceAttackRange(PieceModel piece)
        {
            //check for ranged units first since they can't move and attack
            if (piece == null || !piece.currentPlayerHasControl)
            {
                view.toggleTileFlags(null, TileHighlightStatus.AttackRangeTotal);
                setAttackRangeTiles(null, false);
            }
            else if (piece.isRanged)
            {
                var attackRangeTiles = piece.pieceView.attackRangeTiles;

                view.toggleTileFlags(attackRangeTiles, TileHighlightStatus.AttackRangeTotal);
                if(piece.canAttack){
                    setAttackRangeTiles(attackRangeTiles, !piece.currentPlayerHasControl);
                }
            }
            else if (piece.isMelee)
            {
                //melee units

                var movePositionsTotal = mapService.GetMovementTilesInRadius(piece, true, false);

                List<Tile> attackTiles = null;
                if (piece.canAttack)
                {
                    attackTiles = piece.pieceView.attackRangeTiles;
                }

                setAttackRangeTiles(attackTiles, !piece.currentPlayerHasControl);

                //now find all the attack positions that are possible regardless of can attack or movement
                var movePositionListTotal = movePositionsTotal != null ? movePositionsTotal.Keys.ToList() : new List<Vector2>(){ piece.tilePosition };
                var attackPositionsTotal = mapService.Expand(movePositionListTotal, 1);
                var attackTilesTotal = attackPositionsTotal.Values.ToList();

                attackTilesTotal = attackTilesTotal
                    .Where(t => piece.canAttackTile(pieces, t))
                    .ToList();
                view.toggleTileFlags(attackTilesTotal, TileHighlightStatus.AttackRangeTotal);
            }
        }

        private void updatePieceMoveRange(PieceModel piece)
        {
            if(piece == null || !piece.currentPlayerHasControl)
            {
                view.toggleTileFlags(null, TileHighlightStatus.MoveRange);
                view.toggleTileFlags(null, TileHighlightStatus.MoveRangeTotal);
                return;
            }
            var gameTile = map.tiles.Get(piece.tilePosition);

            //find movement
            var moveTiles = mapService.GetMovementTilesInRadius(piece, false, false);
            var totalMoveTiles = mapService.GetMovementTilesInRadius(piece, true, false);
            //take out the central one
            if(moveTiles != null){
                moveTiles.Remove(gameTile.position);
            }
            if(totalMoveTiles != null){
                totalMoveTiles.Remove(gameTile.position);
            }
            view.toggleTileFlags(moveTiles != null ? moveTiles.Values.ToList() : null, TileHighlightStatus.MoveRange);
            view.toggleTileFlags(totalMoveTiles != null ? totalMoveTiles.Values.ToList() : null, TileHighlightStatus.MoveRangeTotal);
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
            if(piece == null || piece.piece.playerId == players.Me.id ){
                pieceNotDeploying();
            }
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
            updatePieceMoveRange(selectedPiece);
            updatePieceAttackRange(selectedPiece);
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
            var area = model.cardArea;
            if (area != null)
            {
                selectingArea = model;
                if(area.areaTiles.Count > 0)
                {
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
            view.toggleTileFlags(null, TileHighlightStatus.AttackRangeTotal);
            selectingArea = null;
            updateSelectHighlights(null);
            pieceNotDeploying();
        }

        [ListensTo(typeof(SelectTargetSignal))]
        public void onSelectTarget(TargetModel card)
        {
            setAttackRangeTiles(null);
            view.toggleTileFlags(null, TileHighlightStatus.AttackRangeTotal);
            selectingArea = null;
            updateSelectHighlights(null);
        }

        [ListensTo(typeof(MovePathFoundSignal))]
        public void onMovePathFound(MovePathFoundModel mpf)
        {
            movePath = mpf;
        }

        private void updateSelectHighlights(TargetModel model)
        {
            view.toggleTileFlags(null, TileHighlightStatus.TargetTile, true);
            if (model != null )
            {
                if (model.cardArea != null && model.selectedPosition.HasValue)
                {
                    List<Tile> tiles = new List<Tile>();
                    var area = model.cardArea;

                    switch (area.areaType)
                    {
                        case AreaType.Square:
                            tiles.AddRange(mapService.GetKingTilesInRadius(model.selectedPosition.Value, area.size).Values.ToList());
                            break;
                        case AreaType.Diamond:
                            tiles.AddRange(mapService.GetTilesInRadius(model.selectedPosition.Value, area.size).Values.ToList());
                            break;
                        case AreaType.Cross:
                            tiles.AddRange(mapService.GetCrossTiles(model.selectedPosition.Value, area.size).Values.ToList());
                            break;
                        case AreaType.Line:
                            tiles.AddRange(mapService.GetKingTilesInRadius(model.selectedPosition.Value, 1).Values.ToList());
                            break;
                        case AreaType.Row:
                            tiles.AddRange(mapService.GetCrossTiles(model.selectedPosition.Value, 1).Values.ToList());
                            break;
                        case AreaType.Diagonal:
                            tiles.AddRange(mapService.GetDiagonalTilesInRadius(model.selectedPosition.Value, 1).Values.ToList());
                            break;
                    }

                    //This might be a subtle bug here if you have multiple areas one with move restriction and the others without
                    //But for now there shouldn't be any cards like that
                    if(area.moveRestricted){
                        //take out tiles we can't move to
                        tiles = tiles.Where(t => !t.unpassable && pieces.PieceAt(t.position) == null ).ToList();
                    }
                    view.toggleTileFlags(tiles.Distinct().ToList(), TileHighlightStatus.TargetTile, true);
                }
                else if (model.targets != null && model.targets.targetPieceIds.Count > 0)
                {
                }
                else
                {
                    view.toggleTileFlags(map.tileList, TileHighlightStatus.TargetTile, true);
                }

                if(model.targets != null){
                    //update piece target candidates
                    var friendlyTargets = pieces.Pieces
                        .Where(p => p.currentPlayerHasControl && model.targets.targetPieceIds.Contains(p.id))
                        .Select(p => map.tiles[p.tilePosition]).ToList();
                    var enemyTargets = pieces.Pieces
                        .Where(p => !p.currentPlayerHasControl && model.targets.targetPieceIds.Contains(p.id))
                        .Select(p => map.tiles[p.tilePosition]).ToList();
                    view.toggleTileFlags(friendlyTargets, TileHighlightStatus.FriendlyTargetTile);
                    view.toggleTileFlags(enemyTargets, TileHighlightStatus.EnemyTargetTile);
                }
            }else{
                view.toggleTileFlags(null, TileHighlightStatus.FriendlyTargetTile);
                view.toggleTileFlags(null, TileHighlightStatus.EnemyTargetTile);
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

        private List<Tile> GetAreaTiles(AreaTarget area, Vector2 centerPosition){
            if(area.areaTiles != null){
                return area.areaTiles.Select(p => mapService.Tile(p.Vector2)).ToList();
            }
            List<Tile> tiles = new List<Tile>();
            switch (area.areaType) {
                case AreaType.Square:
                    tiles.AddRange(mapService.GetKingTilesInRadius(centerPosition, area.size).Values.ToList());
                    break;
                case AreaType.Diamond:
                    tiles.AddRange(mapService.GetTilesInRadius(centerPosition, area.size).Values.ToList());
                    break;
                case AreaType.Cross:
                    tiles.AddRange(mapService.GetCrossTiles(centerPosition, area.size).Values.ToList());
                    break;
                case AreaType.Line:
                case AreaType.Row:
                case AreaType.Diagonal:
                    if (selectingArea.selectedPosition != null)
                    {
                        tiles.AddRange(mapService.GetLineTiles(
                            selectingArea.selectedPosition.Value,
                            centerPosition,
                            area.size,
                            area.bothDirections ?? false
                        ).Values.ToList());
                    }
                    break;
                case AreaType.Star:
                    var crossTiles = mapService.GetCrossTiles(centerPosition, area.size).Values.ToList();
                    var onePivotPosition = DirectionHelpers.adjacentPosition(centerPosition, Direction.North, true);
                    var oneDiagonal = mapService.GetLineTiles(centerPosition, onePivotPosition, area.size, true);
                    var secondPivotPosition = DirectionHelpers.adjacentPosition(centerPosition, Direction.East, true);
                    var secondDiagonal = mapService.GetLineTiles(centerPosition, secondPivotPosition, area.size, true);
                    tiles.AddRange(crossTiles);
                    tiles.AddRange(oneDiagonal.Values.ToList());
                    tiles.AddRange(secondDiagonal.Values.ToList());
                    tiles = tiles.Distinct().ToList();
                    break;
            }

            return tiles;
        }
    }
}

