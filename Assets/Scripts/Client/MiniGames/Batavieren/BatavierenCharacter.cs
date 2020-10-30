using UnityEngine;

public class BatavierenCharacter : MonoBehaviour {
    [SerializeField]
    private Rigidbody2D rb2d = default;
    [SerializeField]
    private float jumpInterval = 0.5f;
    [SerializeField]
    private float jumpSpeed = 5f;
    [SerializeField]
    private float groundHeight = 1f;

    private float timeSinceLastJump = 0f;
    private bool isAlive = true;

    protected void Update() {
        timeSinceLastJump += Time.deltaTime;
        if (
            timeSinceLastJump > jumpInterval
            && (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space))
            && Physics2D.Raycast(transform.position, Vector2.down, groundHeight, LayerMask.GetMask("Ground")).collider != null
        ) {
            rb2d.AddForce(Vector2.up * jumpSpeed, ForceMode2D.Impulse);
            timeSinceLastJump = 0f;
        }
    }

    public bool IsAlive() {
        return isAlive;
    }

    public void OnTriggerEnter2D(Collider2D collision) {
        isAlive = false;
        rb2d.constraints = RigidbodyConstraints2D.FreezeAll;
        transform.localPosition = Vector3.down * 20;
    }
}