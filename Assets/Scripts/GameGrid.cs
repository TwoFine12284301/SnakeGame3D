using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GameGrid : MonoBehaviour {

    public int gridSize = 5;
    public GameObject gridCubeClone;
    public float rotationDuration = 5.0f;

    private LinkedList<GridCube> snake;
    private List<GridCube> cubes;
    private bool rotationEnabled = false;

    private bool isRotating = false;
    private Vector3 rotationDirection;
    private float startTime = 0;
    private float lastVal = 0;

    public enum MoveResult {
        MOVED, ATE, DIED, ROTATING, ERROR, NONE
    }

    void Update() {
        if (isRotating) {
            float t = (Time.time - startTime) / rotationDuration;
            float newVal = Mathf.SmoothStep(0, 90, t);
            float diff = newVal - lastVal;
            lastVal = newVal;

            transform.Rotate(rotationDirection * diff, Space.World);

            if (t >= 1) {
                isRotating = false;
            }
        }
    }

    public MoveResult MoveHead(GridCube.Direction direction) {
        if (isRotating) {
            return MoveResult.ROTATING;
        }

        bool changedSide = false;
        GridCube next = SnakeHead().GetNextCube(direction, out changedSide);
        if (next == null) {
            return MoveResult.DIED;
        }

        if (next.IsSnake() || next.IsHole()) {
            return MoveResult.DIED;
        }

        if (changedSide) {
            bool ok = StartRotation(direction);
            return ok ? MoveResult.ROTATING : MoveResult.ERROR;
        }

        bool ateApple = next.IsApple();

        next.SetCubeState(GridCube.CubeState.SNAKE);
        snake.AddFirst(next);

        GridCube last = snake.Last.Value;
        if (!ateApple) {
            last.SetCubeState(GridCube.CubeState.EMPTY);
            snake.RemoveLast();
            return MoveResult.MOVED;
        } else {
            return MoveResult.ATE;
        }
    }

    private bool StartRotation(GridCube.Direction direction) {
        Vector3 rotation;
        switch (direction) {
            case GridCube.Direction.UP:
                rotation = new Vector3(-1, 0, 0);
                break;
            case GridCube.Direction.DOWN:
                rotation = new Vector3(1, 0, 0);
                break;
            case GridCube.Direction.LEFT:
                rotation = new Vector3(0, -1, 0);
                break;
            case GridCube.Direction.RIGHT:
                rotation = new Vector3(0, 1, 0);
                break;
            default:
                Debug.LogWarning("Unable to rotate grid!");
                return false;
        }

        rotationDirection = rotation;
        startTime = Time.time;
        lastVal = 0;
        isRotating = true;
        return true;
    }

    public float GetGridSizeWorld() {
        return gridCubeClone.transform.transform.localScale.x * gridSize;
    }

    public void PlaceNewHole() {
        // TODO: Avoid placing holes on edges
        PlaceNewObject(GridCube.CubeState.HOLE);
    }

    public void PlaceNewApple() {
        PlaceNewObject(GridCube.CubeState.APPLE);
    }

    private GridCube SnakeHead() {
        return snake.First.Value;
    }

    private void PlaceNewObject(GridCube.CubeState state) {
        bool done = false;
        while (!done) {
            GridCube cube = cubes.ElementAt(Random.Range(0, cubes.Count));
            if (!cube.isEmpty() || (cube.SameSideAs(SnakeHead()) && rotationEnabled)) {
                continue;
            }

            cube.SetCubeState(state);
            done = true;
        }
    }

    public void SetupGrid(bool enableRotation, int appleCount) {
        if (cubes != null) {
            foreach (GridCube c in cubes) {
                Destroy(c.gameObject);
            }
        }

        snake = new LinkedList<GridCube>();
        cubes = new List<GridCube>();
        isRotating = false;
        rotationEnabled = enableRotation;

        if (gridSize % 2 == 0) {
            gridSize++;
        }

        gridSize = Mathf.Max(gridSize, 5);

        float finalGridSize = GetGridSizeWorld();
        float halfGridSize = finalGridSize / 2;

        int zDepth = rotationEnabled ? gridSize : 1;

        for (int i = 0; i < gridSize; i++) {
            for (int j = 0; j < gridSize; j++) {
                for (int k = 0; k < zDepth; k++) {

                    // Dont add cubes at center of 3d grid
                    if ((k != 0 && k != gridSize - 1) && (j != 0 && j != gridSize - 1) && (i != 0 && i != gridSize - 1)) {
                        continue;
                    }

                    GameObject cubeGameObject = Instantiate(gridCubeClone);
                    cubeGameObject.transform.SetParent(transform);

                    Vector3 size = cubeGameObject.transform.localScale;
                    float offset = halfGridSize - size.x / 2;
                    cubeGameObject.transform.Translate(i * size.x - offset, j * size.x - offset, k * size.x - offset);

                    int centerPos = (int)halfGridSize;
                    GridCube cube = cubeGameObject.GetComponent<GridCube>();

                    if (i == centerPos && j == centerPos && k == 0) {
                        // Set up starting cell 
                        cube.SetCubeState(GridCube.CubeState.SNAKE);
                        snake.AddFirst(cube);
                    } else {
                        cube.SetCubeState(GridCube.CubeState.EMPTY);
                    }

                    if (i == 0) {
                        cube.AddCubeSide(GridCube.CubeSide.LEFT);
                    } else if (i == gridSize - 1) {
                        cube.AddCubeSide(GridCube.CubeSide.RIGHT);
                    }

                    if (j == 0) {
                        cube.AddCubeSide(GridCube.CubeSide.BOTTOM);
                    } else if (j == gridSize - 1) {
                        cube.AddCubeSide(GridCube.CubeSide.TOP);
                    }

                    if (k == 0) {
                        cube.AddCubeSide(GridCube.CubeSide.FRONT);
                    } else if (k == gridSize - 1) {
                        cube.AddCubeSide(GridCube.CubeSide.BACK);
                    }

                    cubes.Add(cube);
                }
            }
        }
        
        for (int i = 0; i < appleCount; i++) {
            PlaceNewApple();
        }
    }
}
