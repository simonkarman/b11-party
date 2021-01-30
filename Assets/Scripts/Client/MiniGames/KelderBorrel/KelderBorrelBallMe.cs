using UnityEngine;

public class KelderBorrelBallMe : KelderBorrelBall {
    private bool isAlive = true;

    protected void Awake() {
        var rb2d = GetComponent<Rigidbody2D>();
        var startVelocity = new Vector2((Random.value - 0.5f) * 2f, 1f);
        rb2d.AddForce(startVelocity.normalized * 5f, ForceMode2D.Impulse);
    }

    public bool IsAlive() {
        return isAlive;
    }

    protected void OnTriggerEnter2D(Collider2D collision) {
        isAlive = false;
    }

    protected void OnCollisionEnter2D(Collision2D collision) {
        var block = collision.gameObject.GetComponent<KelderBorrelBlock>();
        if (block) {
            block.Hit();
        }
    }
}
