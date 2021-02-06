using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ColorShifter))]
public class ConstiCharacter : MonoBehaviour {
    [SerializeField]
    private float moveIntervalDefault;
    [SerializeField]
    private float moveIntervalChasing;
    [SerializeField]
    private Slider slider;

    private KarmanProtocol.KarmanClient client;
    private ColorShifter colorShifter;

    private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    private bool movingEnabled;
    private bool isAlive = true;
    private float moveInterval;
    private float timeSinceLastMove = 0f;
    private bool chasing = false;
    private float chaseTime = 0f;
    private Vector2 moveFrom;
    private Vector2 moveTo;

    public void Initialize(B11PartyClient client) {
        this.client = client.GetKarmanClient();
        colorShifter = GetComponent<ColorShifter>();
        colorShifter.enabled = false;
        moveInterval = moveIntervalDefault;
        moveFrom = transform.localPosition;
        moveTo = transform.localPosition;
    }

    public void SetMovingEnabled(bool movingEnabled) {
        this.movingEnabled = movingEnabled;
    }

    public void StartChasing() {
        chasing = true;
        chaseTime = 0f;
        slider.gameObject.SetActive(true);
        slider.value = 1f;
        colorShifter.enabled = true;
    }

    public void StopChasing() {
        chasing = false;
        slider.value = 0f;
        slider.gameObject.SetActive(false);
        colorShifter.enabled = false;
    }

    public bool IsAlive() {
        return isAlive;
    }

    protected void Update() {
        if (!isAlive) {
            return;
        }

        if (chasing) {
            chaseTime += Time.deltaTime;
            slider.value = 1f - (chaseTime / ConstiServerMiniGame.ChasingDuration);
        }

        timeSinceLastMove += Time.deltaTime;
        bool canMove = movingEnabled && (timeSinceLastMove >= moveInterval);
        if (canMove && TryMoveFromInput()) {
            moveInterval = chasing ? moveIntervalChasing : moveIntervalDefault;
            timeSinceLastMove = 0f;
        }
        float moveT = Mathf.Clamp01(timeSinceLastMove / moveInterval);
        transform.localPosition = Vector2.Lerp(moveFrom, moveTo, moveCurve.Evaluate(moveT));
    }

    public void SetSpawn(Vector3 spawnPosition) {
        transform.position = spawnPosition;
        moveFrom = transform.localPosition;
        moveTo = moveFrom;
    }

    private bool TryMoveFromInput() {
        return ((Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) && CanMoveTowards(Vector2.up))
            || ((Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) && CanMoveTowards(Vector2.left))
            || ((Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) && CanMoveTowards(Vector2.down))
            || ((Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) && CanMoveTowards(Vector2.right));
    }

    private bool CanMoveTowards(Vector2 direction) {
        bool isOccupied = Physics2D.OverlapPoint((Vector2)transform.position + direction, LayerMask.GetMask("Ground"));
        bool moveTowards = !isOccupied;
        if (moveTowards) {
            moveFrom = transform.localPosition;
            moveTo = moveFrom + direction;
            moveTo.x = Mathf.Round(moveTo.x);
            moveTo.y = Mathf.Round(moveTo.y);
        }
        return moveTowards;
    }

    protected void OnTriggerEnter2D(Collider2D collision) {
        if (!isAlive) {
            return;
        }
        Debug.Log($"You hit a '{collision.name}'.", collision.gameObject);

        if (collision.name == "ConstiEnemy") {
            var enemy = collision.transform;
            var enemyScript = enemy.GetComponent<ConstiEnemy>();
            if (!enemyScript.IsDead()) {
                if (chasing) {
                    if (!enemyScript.IsLocalEaten()) {
                        enemyScript.MarkLocalEaten();
                        client.Send(new ConstiEnemyEatenPacket(enemy.GetSiblingIndex()));
                    }
                } else {
                    isAlive = false;
                }
            }
        } else if (collision.name == "switch") {
            var _switch = collision.transform;
            var block = _switch.parent;
            client.Send(new ConstiBlockDisabledPacket(block.GetSiblingIndex()));
        } else if (collision.name == "coin") {
            var coin = collision.transform;
            client.Send(new ConstiCoinUpdatedPacket(coin.GetSiblingIndex(), false));
        } else if (collision.name == "powerup") {
            var powerup = collision.transform;
            client.Send(new ConstiPowerupUpdatedPacket(powerup.GetSiblingIndex(), false));
        } else {
            Debug.LogWarning($"The hit of a '{collision.name}' could not be handled.", collision.gameObject);
        }
    }
}
