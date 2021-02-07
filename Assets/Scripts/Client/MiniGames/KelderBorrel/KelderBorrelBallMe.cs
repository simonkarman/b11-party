using UnityEngine;
using UnityEngine.UI;

public class KelderBorrelBallMe : KelderBorrelBall {
    [SerializeField]
    private Rigidbody2D rb2d;
    [SerializeField]
    private Text holdingText;

    private Transform bar;
    private bool isHolding;
    private float holdTime;
    private readonly float minHoldDuration = 3f;

    public void Initialize(Transform bar) {
        this.bar = bar;
        Hold();
    }

    public void Hold() {
        isHolding = true;
        holdingText.gameObject.SetActive(true);
    }

    public void Release() {
        isHolding = false;
        holdTime = 0f;
        var startVelocity = new Vector2(Random.value - 0.5f, 2f);
        rb2d.velocity = startVelocity.normalized * 5f;
    }

    protected void OnTriggerEnter2D(Collider2D collision) {
        Hold();
    }

    protected void OnCollisionEnter2D(Collision2D collision) {
        var block = collision.gameObject.GetComponent<KelderBorrelBlock>();
        if (block) {
            block.Hit();
        }
    }

    protected void Update() {
        if (isHolding) {
            // Is Holding
            holdTime += Time.deltaTime;
            if (holdTime >= minHoldDuration) {
                holdingText.gameObject.SetActive(false);
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) {
                    Release();
                }
            } else {
                holdingText.text = Mathf.CeilToInt(minHoldDuration - holdTime).ToString();
                holdingText.transform.localScale = Vector3.one * (1.1f - (holdTime % 1f));
            }
            transform.position = bar.position + Vector3.up * 0.5f;
        } else {
            // Is Released
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) {
                Hold();
            }
        }
    }
}
