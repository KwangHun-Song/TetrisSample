using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace TetrisEngine {
    public enum BlockType { I, L, Lr, N, Nr, T, O } // r은 reversed를 뜻함(뒤집어진 상태)
    public enum GameResult { Clear, Fail }
    public enum Direction { Left, Right, Down }
    public enum InputType { Left, Right, Down, Rotate, Skip, Escape }

    public class Controller {
        public Queue<BlockType> blocksQueue;
        private float fallInterval;
        private CancellationTokenSource fallIntervalTokenSource;
        private CancellationTokenSource gameTokenSource;

        public Block ActiveBlock { get; set; }
        public Tile[] Board { get; set; } // true는 Occupied, false는 empty
        
        private Queue<InputType> InputsQueue { get; } = new Queue<InputType>();
        private bool OnProcessInput { get; set; }
        private bool SkipPressed { get; set; }

        public async UniTask<GameResult> StartGame(Level level) {
            // 초기화
            blocksQueue = new Queue<BlockType>(level.spawnBlocks);
            fallInterval = level.fallInterval;
            Board = Enumerable.Range(0, Const.BoardWidth * Const.BoardHeight).Select(i => new Tile(i)).ToArray();
            ActiveBlock = null;
            gameTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(10));

            while ((blocksQueue.Any() || ActiveBlock != null) && gameTokenSource.IsCancellationRequested == false) {
                // 현재 움직이는 블럭이 없으면 스폰한다.
                if (ActiveBlock == null) {
                    // 스폰할 수 없으면 패배 처리
                    if (TrySpawnBlock(blocksQueue.Dequeue(), out var block) == false) return GameResult.Fail;
                    ActiveBlock = block;
                }

                // 인터벌동안 인풋을 받는다.
                if (SkipPressed == false)
                    fallIntervalTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(fallInterval));
                try {
                    await UniTask.WaitUntilCanceled(fallIntervalTokenSource.Token);
                }
                catch (OperationCanceledException) { } // 인터벌 완료, 에러 뱉지 않고 다음 스텝으로

                // 인터벌이 끝난 후 처리
                if (await TryMoveAsync(Direction.Down)) continue;
                
                await FixAsync(); // 움직이는 블럭이 아래로 내려갈 수 없으면 고정시킨다.
                await DestroyOccupiedRowsAsync(); // 꽉 찬 행이 있으면 파괴한다.
                ActiveBlock = null;
                SkipPressed = false;
                    
                // 게임 판정
                if (blocksQueue.Any() == false) return GameResult.Clear;
                if (Board.Any(t => t.Occupied && t.Y == Const.BoardHeight - 1)) return GameResult.Fail;
            }

            return GameResult.Fail;
        }

        public void Input(InputType inputType) {
            if (fallIntervalTokenSource?.IsCancellationRequested ?? true) return;
            InputsQueue.Enqueue(inputType);
            ProcessInputQueueAsync().Forget();
        }

        private async UniTask ProcessInputQueueAsync() {
            if (OnProcessInput) return;
            OnProcessInput = true;

            while (InputsQueue.Any()) {
                switch (InputsQueue.Dequeue()) {
                    case InputType.Left:
                        await TryMoveAsync(Direction.Left);
                        break;
                    case InputType.Right:
                        await TryMoveAsync(Direction.Right);
                        break;
                    case InputType.Rotate:
                        await TryRotateAsync();
                        break;
                    case InputType.Down:
                        fallIntervalTokenSource.Cancel();
                        break;
                    case InputType.Skip:
                        fallIntervalTokenSource.Cancel();
                        SkipPressed = true;
                        break;
                    case InputType.Escape:
                        gameTokenSource.Cancel();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            OnProcessInput = false;
        }

        private bool TrySpawnBlock(BlockType blockType, out Block spawnBlock) {
            var blockHeight = Utility.GetBlockHeight(blockType);
            var blockStartX = Const.BoardWidth / 2;
            var blockStartY = Const.BoardHeight - blockHeight;
            spawnBlock = new Block(blockStartX, blockStartY, blockType);
            return CanPut(spawnBlock);
        }

        private bool CanPut(Block block) {
            foreach ((int x, int y) in block.GetOccupiedIndices()) {
                if (x < 0) return false;
                if (x > Const.BoardWidth - 1) return false;
                if (y < 0) return false;
                if (y > Const.BoardHeight - 1) return false;
                if (Board[(x, y).GetIndex()].Occupied) return false;
            }

            return true;
        }

        private async UniTask<bool> TryMoveAsync(Direction direction) {
            var newBlock = ActiveBlock.Clone();
            switch (direction) {
                case Direction.Left:
                    newBlock.X--;
                    break;
                case Direction.Right:
                    newBlock.X++;
                    break;
                case Direction.Down:
                    newBlock.Y--;
                    break;
            }

            if (CanPut(newBlock) == false) return false;
            ActiveBlock = newBlock;
            return true;
        }

        private async UniTask<bool> TryRotateAsync() {
            var newBlock = ActiveBlock.Clone();
            newBlock.Rotate90();

            if (CanPut(newBlock) == false) return false;
            ActiveBlock = newBlock;
            return true;
        }

        private async UniTask FixAsync() {
            foreach ((int x, int y) in ActiveBlock.GetOccupiedIndices()) {
                Board[(x, y).GetIndex()].Occupied = true;
            }
        }

        private async UniTask DestroyOccupiedRowsAsync() {
            for (int row = Const.BoardHeight - 1; row >= 0; row--) {
                var rowTiles = Board.Where(t => t.Y == row).ToArray();
                if (!rowTiles.All(t => t.Occupied)) continue;

                // 현재 row와 같거나 높은 위치에 있는 모든 타일들이, 한 칸 위의 타일의 상태를 덮어쓴다.
                var portTiles = Board.Where(t => t.Y >= row);
                foreach (var tile in portTiles) {
                    var isTopTile = tile.Y == Const.BoardHeight - 1;
                    if (isTopTile) {
                        tile.Occupied = false;
                    } else {
                        var upTile = Board[(tile.X, tile.Y + 1).GetIndex()];
                        tile.Occupied = upTile.Occupied;
                    }
                }
            }
        }
    }
}