using System;
using UnityEngine;

public class ClientLobbyCharacter : MonoBehaviour {
    [SerializeField]
    private Rigidbody2D rigidbody2d = default;
    [SerializeField]
    private float moveSpeed = 3f;

    private bool chosen = false;
    private string closestMiniGame = null;

    public Action<string> OnChosenMiniGameCallback;

    protected void Start() {
        Reset();
    }

    public void Reset() {
        chosen = false;
        closestMiniGame = null;
    }

    protected void FixedUpdate() {
        if (!chosen) {
            Vector2 input = new Vector2(
                Input.GetAxis("Horizontal"),
                Input.GetAxis("Vertical")
            );
            rigidbody2d.AddForce(input * moveSpeed);
        }
    }

    protected void Update() {
        if (!chosen && closestMiniGame != null && Input.GetKeyDown(KeyCode.Space)) {
            chosen = true;
            rigidbody2d.velocity = Vector2.zero;
            OnChosenMiniGameCallback(closestMiniGame);
        }
    }

    public void SetClosestMiniGame(string closestMiniGame) {
        this.closestMiniGame = closestMiniGame;
    }
}