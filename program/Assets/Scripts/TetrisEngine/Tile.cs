namespace TetrisEngine {
    public class Tile {
        public int Index { get; }
        public int X => Index % Const.BoardWidth;
        public int Y => Index / Const.BoardWidth;
        public bool Occupied;
        
        public Tile(int index) {
            Index = index;
        }
    }
}