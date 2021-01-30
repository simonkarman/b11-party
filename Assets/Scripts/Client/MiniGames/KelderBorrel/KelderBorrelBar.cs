using UnityEngine;

public class KelderBorrelBar : MonoBehaviour {
    [SerializeField]
    private float moveSpeed = 1f;

    protected void Update() {
        float input = Input.GetAxis("Horizontal");
        transform.localPosition += Vector3.right * input * moveSpeed * Time.deltaTime;
    }
}
