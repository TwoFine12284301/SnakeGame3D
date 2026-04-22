using UnityEngine;
using System.Collections;
using System;

public class GridCube : MonoBehaviour {

    private CubeState currentState = CubeState.EMPTY;
    public CubeSide cubeSide = 0;

//------Make the apples and snake 3d---------|
    private GameObject floatingApple = null;
    private GameObject floatingSnake = null;
//-------------------------------------------|

//--------Create the arrows in the edges-----|
    private static edgeManager _edgeManager;
    private static edgeManager edgeManager {
        get {
            if (_edgeManager == null) 
                _edgeManager = FindFirstObjectByType<edgeManager>();
                return _edgeManager;
            
        }
    }
//------------------------------------------|
    [Flags]
    public enum CubeSide {FRONT = 1,BACK = 2,TOP = 4,BOTTOM = 8, LEFT = 16,RIGHT = 32}

    public enum Direction {UP, DOWN, LEFT, RIGHT, NONE}

    public enum CubeState {SNAKE, APPLE, EMPTY, HOLE}

    public void AddCubeSide(CubeSide s) {cubeSide |= s;}

    public bool SameSideAs(GridCube other) {return (other.cubeSide & cubeSide) != 0;}



    
//-----Get out normal vector for the position of the apples-------------
    private Vector3 normalVec(){
        Vector3 v = transform.localPosition;
        float ax = Mathf.Abs(v.x);
        float ay = Mathf.Abs(v.y);
        float az = Mathf.Abs(v.z);

        float maxDistance = Mathf.Max(ax, Mathf.Max(ay, az));
        Vector3 normal = Vector3.zero;
        float bestDot = -999f;
        Camera cam = Camera.main;

        //Check the azes that are in the edge
        if (ax >= maxDistance - 0.01f) {
            Vector3 n = new Vector3(Mathf.Sign(v.x), 0, 0);
            float dot = Vector3.Dot(transform.parent.TransformDirection(n), cam.transform.position - transform.position);
            if (dot > bestDot) { bestDot = dot; normal = n; }
        }
        if (ay >= maxDistance - 0.01f) {
            Vector3 n = new Vector3(0, Mathf.Sign(v.y), 0);
            float dot = Vector3.Dot(transform.parent.TransformDirection(n), cam.transform.position - transform.position);
            if (dot > bestDot) { bestDot = dot; normal = n; }
        }
        if (az >= maxDistance - 0.01f) {
            Vector3 n = new Vector3(0, 0, Mathf.Sign(v.z));
            float dot = Vector3.Dot(transform.parent.TransformDirection(n), cam.transform.position - transform.position);
            if (dot > bestDot) { bestDot = dot; normal = n; }
        }

        return normal;
    }

//------------------------------------------------------------------------
private static Material snakeMat;
private static Material AppleMat;

    public void SetCubeState(CubeState state) {
        Renderer ren = GetComponent<MeshRenderer>();
        currentState = state;

        if (state != CubeState.APPLE){
            if(floatingApple != null) {
                Destroy(floatingApple);
                floatingApple = null;
                }
                edgeManager?.UnregisterApple(this);
        }

        //--Clean up the snake--------------|
        if (state != CubeState.SNAKE){
            if (floatingSnake != null){
                Destroy(floatingSnake);
                floatingSnake = null;
            }
        }
        switch (state) {
            case CubeState.SNAKE:
                ren.enabled = true;
                ren.material.color = new Color(0.56f, 0.93f, 0.56f);

                //------Make the snake 3d----------|
                if (floatingSnake == null){
                    floatingSnake = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    floatingSnake.transform.SetParent(this.transform);

                    floatingSnake.transform.localScale = Vector3.one * 0.85f;
                    floatingSnake.transform.localPosition = normalVec() * 0.5f;

                    Destroy(floatingSnake.GetComponent<Collider>());
                }

                //----Creating the material for the snake cubes-------|
                if (snakeMat == null){
                    snakeMat = new Material(Shader.Find("Standard"));
                    snakeMat.color = new Color(0.1f, 0.5f, 0.1f, 1f);
                    snakeMat.SetFloat("_Metallic", 0.1f);
                    snakeMat.SetFloat("_Glossiness", 0.4f);
                }
                
                floatingSnake.GetComponent<Renderer>().material = snakeMat;
                break;

            case CubeState.APPLE:
                ren.enabled = true;
                ren.material.color = new Color(0.56f, 0.93f, 0.56f);
                // Spawn the 3D Sphere
                if (floatingApple == null) {
                    floatingApple = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    floatingApple.transform.SetParent(this.transform);

                    //Get the position
                    floatingApple.transform.localScale = Vector3.one * 0.62f; 
                    floatingApple.transform.localPosition = normalVec() * 0.4f;

                    Destroy(floatingApple.GetComponent<Collider>()); 
                    
                    // Change the texture of the apple
                    if (AppleMat == null){
                        AppleMat = new Material(Shader.Find("Standard"));
                        AppleMat.color = new Color(0.78f, 0.06f, 0.06f, 1f);
                        AppleMat.SetFloat("_Metallic", 0.55f);
                        AppleMat.SetFloat("_Glossiness", 0.82f);
                    }
                    
                    floatingApple.GetComponent<Renderer>().material = AppleMat;
                }
                edgeManager?.RegisterApple(this);
                break;

            case CubeState.HOLE:
                ren.enabled = true;
                ren.material.color = Color.black;
                break;
            case CubeState.EMPTY:
            default:
                ren.enabled = true;
                ren.material.color = new Color(0.56f, 0.93f, 0.56f);
                break;
        }
    }


    public bool IsApple() {
        return currentState == CubeState.APPLE;
    }
    public bool IsHole() {
        return currentState == CubeState.HOLE;
    }
    public bool IsSnake() {
        return currentState == CubeState.SNAKE;
    }
    public bool isEmpty() {
        return currentState == CubeState.EMPTY;
    }

    public GridCube GetNextCube(Direction dir, out bool changedSide) {
        changedSide = false;
        Vector3 direction;

        switch (dir) {
            case Direction.UP:
                direction = new Vector3(0, 1, 0);
                break;
            case Direction.DOWN:
                direction = new Vector3(0, -1, 0);
                break;
            case Direction.LEFT:
                direction = new Vector3(-1, 0, 0);
                break;
            case Direction.RIGHT:
                direction = new Vector3(1, 0, 0);
                break;
            default:
                return null;
        }

        GridCube neighbour = GetNeighbourAt(direction);
        if (neighbour == null) {
            // Get neighbour on the other side of the cube (back)
            changedSide = true;
            return GetNeighbourAt(new Vector3(0, 0, 1));
        }

        return neighbour;
    }

    private GridCube GetNeighbourAt(Vector3 direction) {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, direction);
        if (Physics.Raycast(ray, out hit)) {
            GameObject go = hit.collider.gameObject;
            return go.GetComponent<GridCube>();
        }

        return null;
    }
	
	// Update is called once per frame
	void Update () {
	if (currentState == CubeState.APPLE && floatingApple != null) {
            float s = 0.65f * (1f + 0.10f * Mathf.Sin(Time.time * 4f));
            floatingApple.transform.localScale = Vector3.one * s;

            float hover = 0.38f + 0.07f * Mathf.Sin(Time.time * 2f);
            floatingApple.transform.localPosition = normalVec() * hover;

            if (currentState == CubeState.SNAKE && floatingSnake != null){
                floatingSnake.transform.localPosition = normalVec()*0.5f;
            }
	}
    }
}

