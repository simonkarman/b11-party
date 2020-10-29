using UnityEngine;

public class DisableCanvasOnTab : MonoBehaviour {
    protected void Update() {
        if (Input.GetKeyDown(KeyCode.Tab)) {
            Canvas canvas = GetComponent<Canvas>();
            canvas.enabled = !canvas.enabled;
        }
    }
}