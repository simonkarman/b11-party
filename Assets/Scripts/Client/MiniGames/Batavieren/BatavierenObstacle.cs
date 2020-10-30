using UnityEngine;

public class BatavierenObstacle : MonoBehaviour {
    private float speed;

    public void SetSpeed(float speed) {
        this.speed = speed;
    }

    protected void FixedUpdate() {
        transform.position += Time.deltaTime * speed * Vector3.left;
    }
}