using System;

namespace TetrisEngine {
    public static class Utility {
        private static BlockType[] allBlockTiles;
        public static BlockType[] GetAllBlockTypes => allBlockTiles ??= (BlockType[]) Enum.GetValues(typeof(BlockType));
            
        public static (int x, int y) GetCoordi(this int index) {
            return (index % Const.BoardWidth, index / Const.BoardWidth);
        }

        public static int GetIndex(this (int x, int y) coordi) {
            return coordi.y * Const.BoardWidth + coordi.x;
        }

        public static bool[,] GetShape(BlockType blockType) {
            return blockType switch {
                BlockType.I => new bool[,] {
                    {true, false, false, false},
                    {true, false, false, false},
                    {true, false, false, false},
                    {true, false, false, false},
                },
                BlockType.L => new bool[,] {
                    {true, false, false},
                    {true, false, false},
                    {true, true, false},
                },
                BlockType.Lr => new bool[,] {
                    {false, true, false},
                    {false, true, false},
                    {true, true, false},
                },
                BlockType.N => new bool[,] {
                    {false, true, false},
                    {true, true, false},
                    {true, false, false},
                },
                BlockType.Nr => new bool[,] {
                    {true, false, false},
                    {true, true, false},
                    {false, true, false},
                },
                BlockType.T => new bool[,] {
                    {true, false, false},
                    {true, true, false},
                    {true, false, false},
                },
                BlockType.O => new bool[,] {
                    {true, true},
                    {true, true},
                },
            };
        }

        public static int GetBlockHeight(BlockType blockType) {
            return blockType switch {
                BlockType.I => 4,
                BlockType.L => 3,
                BlockType.Lr => 3,
                BlockType.N => 3,
                BlockType.Nr => 3,
                BlockType.T => 3,
                BlockType.O => 2,
            };
        }

        public static bool[,] Rotate90(this bool[,] origin) {
            var rows = origin.GetLength(0);
            var cols = origin.GetLength(1);
            var newArr = new bool[rows, cols];

            for (int y = 0; y < rows; y++) {
                for (int x = 0; x < cols; x++) {
                    newArr[y, rows - 1 - x] = origin[x, y];
                }
            }

            return newArr;
        }

        public static Block Clone(this Block block) {
            return new Block(block.X, block.Y, block.BlockType) {
                Degree = block.Degree
            };
        }
    }
}