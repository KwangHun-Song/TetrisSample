using System.Collections.Generic;

namespace TetrisEngine {
    public class Block {
        public enum BlockDegree { D0, D90, D180, D270 }
    
        public BlockType BlockType { get; }
        public int Index => Y * Const.BoardWidth + X;
        
        public int X { get; set; }
        public int Y { get; set; }
        
        public BlockDegree Degree { get; set; }
        
        public Block(int x, int y, BlockType blockType) {
            BlockType = blockType;
            X = x;
            Y = y;
        }

        public bool[,] GetShape() {
            return Degree switch {
                BlockDegree.D0 => Utility.GetShape(BlockType),
                BlockDegree.D90 => Utility.GetShape(BlockType).Rotate90(),
                BlockDegree.D180 => Utility.GetShape(BlockType).Rotate90().Rotate90(),
                BlockDegree.D270 => Utility.GetShape(BlockType).Rotate90().Rotate90().Rotate90(),
            };
        }

        public IEnumerable<(int x, int y)> GetOccupiedIndices() {
            var shape = GetShape();
            for (int y = 0; y < shape.GetLength(0); y++) {
                for (int x = 0; x < shape.GetLength(1); x++) {
                    if (shape[y, x] == false) continue;
                    yield return (x + X, y + Y);
                }
            }
        }

        public void Rotate90() {
            Degree = Degree switch {
                BlockDegree.D0 => BlockDegree.D90,
                BlockDegree.D90 => BlockDegree.D180,
                BlockDegree.D180 => BlockDegree.D270,
                BlockDegree.D270 => BlockDegree.D0,
            };
        }
    }
}