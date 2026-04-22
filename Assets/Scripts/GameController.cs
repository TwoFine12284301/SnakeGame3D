using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {
    private const float DEFAULT_INPUT_COOLDOWN = 0.2f;
    private const float COOLDOWN_STEP = 0.001f;
    private const float MIN_INPUT_COOLDOWN = 0.05f;
    private const float NEW_HOLE_PROBABILITY = 0.1f;

    public GameGrid gameGrid;
    public GUIController guiController;
    public edgeManager edgeManager;

    private GridCube.Direction lastDirection = GridCube.Direction.RIGHT;
    private GridCube.Direction lastMovedDirection = GridCube.Direction.NONE;
    private GameGrid.MoveResult lastResult;
    private float lastInputTime = 0;
    private int score = 0;
    private bool playing = true;
    private bool rotationEnabled;
    private float inputCoolDown = DEFAULT_INPUT_COOLDOWN;

    void Start() {
        Initialize();
    }

    private void Initialize() {
        rotationEnabled = (PlayerPrefs.GetInt("3dMode", 1) == 1);
        int appleCount = PlayerPrefs.GetInt("AppleCount", 20);

        lastResult = GameGrid.MoveResult.NONE;
        inputCoolDown = DEFAULT_INPUT_COOLDOWN;
        guiController.SetTopScore(PlayerPrefs.GetInt("TopScore", 0));

        gameGrid.SetupGrid(rotationEnabled, appleCount);
        SetupCamera();
    }

    void Update() {
        if (!playing) {
            return;
        }

        GridCube.Direction dir = ReadInput();

        if (dir == GridCube.Direction.NONE || AreOpposite(dir, lastMovedDirection)) {
            dir = lastDirection;
        }

        if (lastResult == GameGrid.MoveResult.ROTATING) {
            dir = lastMovedDirection;
        }

        lastDirection = dir;

        lastInputTime += Time.deltaTime;
        if (lastInputTime > inputCoolDown) {
            
            lastInputTime = 0;

            GameGrid.MoveResult result = gameGrid.MoveHead(dir);

            if (result == GameGrid.MoveResult.MOVED || result == GameGrid.MoveResult.ATE) {
                lastMovedDirection = dir;
            }

            switch (result) {
                case GameGrid.MoveResult.DIED:
                    playing = false;

                    int topScore = PlayerPrefs.GetInt("TopScore", 0);
                    if (score > topScore) {
                        PlayerPrefs.SetInt("TopScore", score);
                    }

                    guiController.RemoveNotifications();
                    guiController.SetGameOverPanelActive(true);
                    break;
                case GameGrid.MoveResult.ERROR:
                    Debug.Log("An error occured.");
                    gameObject.SetActive(false);
                    break;
                case GameGrid.MoveResult.ATE:
                    gameGrid.PlaceNewApple();
                    if (rotationEnabled && Random.value < NEW_HOLE_PROBABILITY) {
                        gameGrid.PlaceNewHole();
                    }

                    //TODO: Win if no more space is available

                    score++;                    
                    guiController.SetScore(score);

                    inputCoolDown -= COOLDOWN_STEP;
                    if (inputCoolDown < MIN_INPUT_COOLDOWN) {
                        inputCoolDown = MIN_INPUT_COOLDOWN;
                    }

                    break;
                case GameGrid.MoveResult.ROTATING:
                default:
                    // pass
                    break;
            }

            lastResult = result;
        }
	}

    void SetupCamera() {
        float frustumHeight = gameGrid.GetGridSizeWorld();
        float distance = frustumHeight / Mathf.Tan(Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad);
        Camera.main.transform.position = new Vector3(0, 0, -distance);
    }

    private bool AreOpposite(GridCube.Direction a, GridCube.Direction b) {
        if ((a == GridCube.Direction.DOWN && b == GridCube.Direction.UP) || 
            (a == GridCube.Direction.UP && b == GridCube.Direction.DOWN)) {
            return true;
        }

        if ((a == GridCube.Direction.RIGHT && b == GridCube.Direction.LEFT) ||
            (a == GridCube.Direction.LEFT && b == GridCube.Direction.RIGHT)) {
            return true;
        }

        return false;
    }

    private GridCube.Direction ReadInput() {
        if (Input.GetKey(KeyCode.UpArrow)) {
            return GridCube.Direction.UP;
        } else if (Input.GetKey(KeyCode.DownArrow)) {
            return GridCube.Direction.DOWN;
        } else if (Input.GetKey(KeyCode.RightArrow)) {
            return GridCube.Direction.RIGHT;
        } else if (Input.GetKey(KeyCode.LeftArrow)) {
            return GridCube.Direction.LEFT;
        }

        return GridCube.Direction.NONE;
    }

    public void RestartGame() {
        guiController.SetGameOverPanelActive(false);
        Initialize();
        playing = true;
        score = 0;
        guiController.SetScore(score);
    }

    public void BackToMenu() {
        SceneManager.LoadScene("Menu");
    }
}
