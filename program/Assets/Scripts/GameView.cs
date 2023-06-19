using System.Linq;
using Cysharp.Threading.Tasks;
using TetrisEngine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameView : MonoBehaviour {
    [SerializeField] private Transform tileViewRoot;
    [SerializeField] private TMP_Text gameStatusText;
    [SerializeField] private Transform nextTilesRoot;

    private Image[] tileViews;
    private Image[] nextTiles;
    private Controller Controller { get; set; }

    private void Awake() {
        tileViews = tileViewRoot.GetComponentsInChildren<Image>();
        nextTiles = nextTilesRoot.GetComponentsInChildren<Image>();
        gameStatusText.text = "Press Enter to start";
    }

    public async UniTask StartGame() {
        gameStatusText.text = "";
        Controller = new Controller();
        var level = new Level {
            spawnBlocks = Enumerable.Repeat(0, 90).Select(_ => GetRandomBlockType()).ToArray(),
            fallInterval = 0.5F
        };
        var result = await Controller.StartGame(level);

        gameStatusText.text = $"{result}";

        BlockType GetRandomBlockType() {
            return Utility.GetAllBlockTypes[Random.Range(0, Utility.GetAllBlockTypes.Length)];
        }
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Return)) {
            StartGame().Forget();
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            Controller?.Input(InputType.Left);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            Controller?.Input(InputType.Right);
        }

        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            Controller?.Input(InputType.Rotate);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            Controller?.Input(InputType.Down);
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            Controller?.Input(InputType.Skip);
        }

        if (Input.GetKeyDown(KeyCode.Escape)) {
            Controller?.Input(InputType.Escape);
        }

        if (Controller != null) {
            foreach (var tile in Controller.Board) {
                tileViews[tile.Index].color = tile.Occupied ? Color.gray : Color.white;
            }

            if (Controller.ActiveBlock != null) {
                foreach ((int x, int y) in Controller.ActiveBlock.GetOccupiedIndices()) {
                    tileViews[(x, y).GetIndex()].color = Color.red;
                }
            }

            foreach (var tileView in nextTiles) {
                tileView.color = new Color(1, 1, 1, 0);
            }
            
            if (Controller.blocksQueue.Any()) {
                var nextTile = Controller.blocksQueue.Peek();
                var shape = Utility.GetShape(nextTile);
                
                for (int y = 0; y < shape.GetLength(0); y++) {
                    for (int x = 0; x < shape.GetLength(1); x++) {
                        if (shape[y, x] == false) continue;
                        nextTiles[y * 4 + x].color = Color.white;
                    }
                }
            }
        }
    }
}