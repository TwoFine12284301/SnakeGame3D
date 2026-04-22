using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class NotificationFade : MonoBehaviour {
    public float duration = 1;
    private float startTime = 0;

    private float maxY = 0;
    private float initialY = 0;
    private RectTransform rectTransform;
    private Text text;

    public void SetupAnimation() {
        rectTransform = GetComponent<RectTransform>();
        text = GetComponent<Text>();

        startTime = Time.time;
        maxY = transform.parent.GetComponent<RectTransform>().rect.height / 2;
        initialY = rectTransform.localPosition.y;
    }

	void Update() {
        float t = (Time.time - startTime) / duration;
        float val = (t * t) * (maxY - initialY);
        float alpha = 1 - (t * t);
        val += initialY;
        rectTransform.localPosition = new Vector3(0, val);

        Color prev = text.color;
        prev.a = alpha;
        text.color = prev;

        if (t >= 1) {
            Destroy(gameObject);
        }
    }
}
