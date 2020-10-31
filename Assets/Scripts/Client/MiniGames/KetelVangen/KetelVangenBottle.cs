using UnityEngine;

public class KetelVangenBottle : MonoBehaviour {
    [SerializeField]
    private int score = default;

    private float speed;

    public void SetSpeed(float speed) {
        this.speed = speed;
    }

    protected void FixedUpdate() {
        transform.position += Time.deltaTime * speed * Vector3.down;
        if (transform.position.y > 100) {
            Destroy(this);
        }
    }

    public int GetScore() {
        return score;
    }
}