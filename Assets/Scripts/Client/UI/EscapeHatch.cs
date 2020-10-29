using UnityEngine;
using UnityEngine.SceneManagement;

public class EscapeHatch : MonoBehaviour {
    private float escapeTime = 1.2f;
    protected void Update() {
        if (Input.GetKey(KeyCode.Escape)) {
            escapeTime -= Time.deltaTime;
            if (escapeTime < 0f) {
                SceneManager.LoadScene("Client");
            }
        } else {
            escapeTime = 1.2f;
        }
    }
}