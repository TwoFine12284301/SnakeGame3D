using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class edgeManager : MonoBehaviour {

    [Header("Appearance")]
    public float edgePadding = 48f;
    public float arrowSize   = 44f;
    public Color arrowColor  = new Color(1f, 0.15f, 0.10f, 1f);

    private Camera mainCam;
    private Canvas gameCanvas;
    private RectTransform canvasRT;

    private RectTransform[] arrowRT   = new RectTransform[4];
    private TextMeshProUGUI[] arrowTxt = new TextMeshProUGUI[4];

    //----Top Bottom Left Right in this order ----------|
    private static readonly Vector2[] anchorPos = {new Vector2( 0f,  1f), new Vector2( 0f, -1f), new Vector2(-1f,  0f), new Vector2( 1f,  0f),};
    private static readonly float[] arrowAngles = {0f,180f,90f, -90f, };

    private readonly List<GridCube> apples = new List<GridCube>();

    //-------------------------------------------------------------------|

    private bool EnsureCanvas() {
        if (gameCanvas != null) return true;
        gameCanvas = Object.FindFirstObjectByType<Canvas>();
        if (gameCanvas != null)
            canvasRT = gameCanvas.GetComponent<RectTransform>();
        return gameCanvas != null;
    }

    void Awake() {
        mainCam = Camera.main;
        EnsureCanvas();
        BuildArrows();
    }

    private void BuildArrows() {
        if (!EnsureCanvas()) return;

        string[] labels = { "Top", "Bottom", "Left", "Right" };

        for (int i = 0; i < 4; i++) {
            GameObject go = new GameObject("EdgeArrow_" + labels[i]);
            go.transform.SetParent(gameCanvas.transform, false);

            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(arrowSize, arrowSize);
            rt.localRotation = Quaternion.Euler(0f, 0f, arrowAngles[i]);

            TextMeshProUGUI txt = go.AddComponent<TextMeshProUGUI>();
            txt.text  = "▲";
            txt.fontSize  = arrowSize;
            txt.color = arrowColor;
            txt.alignment = TextAlignmentOptions.Center;

            go.SetActive(false);

            arrowRT[i]  = rt;
            arrowTxt[i] = txt;
        }
    }

    // -------------------------------------------------------------------------

    public void RegisterApple(GridCube apple) {
        if (apple == null) return;
        if (!apples.Contains(apple))
            apples.Add(apple);
    }

    public void UnregisterApple(GridCube apple) {
        apples.Remove(apple);
    }

    // -------------------------------------------------------------------------

    void Update() {
        if (mainCam == null || gameCanvas == null) return;

        // delete empty entries
        for (int i = apples.Count - 1; i >= 0; i--)
            if (apples[i] == null || !apples[i].IsApple())
                apples.RemoveAt(i);

        // Reset — assume all four arrows off
        bool[] sideActive = { false, false, false, false };

        float halfW = canvasRT.rect.width  * 0.5f;
        float halfH = canvasRT.rect.height * 0.5f;

        foreach (GridCube apple in apples) {
            Vector3 vp = mainCam.WorldToViewportPoint(apple.transform.position);

            Vector3 outVector = apple.transform.parent.TransformDirection(apple.transform.localPosition.normalized);
            Vector3 toCamera = mainCam.transform.position - apple.transform.position;

            bool FacingCamera = Vector3.Dot(outVector, toCamera) > 0.1f;

            bool onScreen = vp.z > 0f
                         && vp.x > 0.05f && vp.x < 0.95f
                         && vp.y > 0.05f && vp.y < 0.95f
                         && FacingCamera;

            if (onScreen) continue;

            // Flip direction if behind camera
            if (vp.z < 0f) { vp.x = 1f - vp.x; vp.y = 1f - vp.y; }

            Vector2 dir = new Vector2(vp.x - 0.5f, vp.y - 0.5f);
            if (dir.sqrMagnitude < 0.0001f) dir = Vector2.up;

            // Determine dominant side
            if (Mathf.Abs(dir.y) >= Mathf.Abs(dir.x)) {
                if (dir.y > 0) sideActive[0] = true;   // Top
                else sideActive[1] = true;   // Bottom
            } else {
                if (dir.x < 0) sideActive[2] = true;   // Left
                else sideActive[3] = true;   // Right
            }
        }

        // Position and pulse each active arrow
        float alpha = 0.55f + 0.45f * Mathf.Abs(Mathf.Sin(Time.time * 2.8f));

        for (int i = 0; i < 4; i++) {
            arrowRT[i].gameObject.SetActive(sideActive[i]);

            if (sideActive[i]) {
                // Pin to the correct edge center
                Vector2 pos = anchorPos[i] * new Vector2(halfW - edgePadding, halfH - edgePadding);
                arrowRT[i].anchoredPosition = pos;

                Color c = arrowColor;
                c.a = alpha;
                arrowTxt[i].color = c;
            }
        }
    }
}
