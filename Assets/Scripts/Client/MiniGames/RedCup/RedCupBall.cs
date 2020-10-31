using System;
using UnityEngine;

public class RedCupBall : MonoBehaviour {
    [SerializeField]
    private Rigidbody2D rb2d = default;

    public Action<int> OnDone;

    private bool isShooting = false;
    private RedCupCup cup;

    public void Shoot(float speed, Vector2 direction) {
        rb2d.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb2d.velocity = direction * speed;
        cup = null;
        isShooting = true;
    }

    protected void Update() {
        if (isShooting && rb2d.velocity.magnitude < 0.3f) {
            isShooting = false;
            rb2d.constraints = RigidbodyConstraints2D.FreezeAll;
            OnDone(cup == null ? -1 : cup.GetCupId());
        }
    }

    protected void OnTriggerEnter2D(Collider2D collision) {
        cup = collision.GetComponent<RedCupCup>();
    }

    protected void OnTriggerExit2D(Collider2D collision) {
        cup = null;
    }
}