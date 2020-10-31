using System;
using UnityEngine;

public class KetelVangenCharacter : MonoBehaviour {
    [SerializeField]
    private Rigidbody2D rb2d = default;
    [SerializeField]
    private float moveSpeed = 5f;

    private bool isCatching = true;
    public Action<int> OnHitBottle;

    protected void Update() {
        if (isCatching) {
            float horizontal = Input.GetAxis("Horizontal");
            rb2d.AddForce(Vector3.right * moveSpeed * Time.deltaTime * horizontal, ForceMode2D.Impulse);
        }
    }

    public void OnTriggerEnter2D(Collider2D collision) {
        Debug.Log("Hit bottle!");
        if (isCatching) {
            KetelVangenBottle bottle = collision.transform.GetComponent<KetelVangenBottle>();
            if (bottle != null) {
                OnHitBottle(bottle.GetScore());
                Destroy(bottle.gameObject);
            }
        }
    }

    public void DisableCatching() {
        isCatching = false;
        rb2d.constraints = RigidbodyConstraints2D.FreezeAll;
    }
}