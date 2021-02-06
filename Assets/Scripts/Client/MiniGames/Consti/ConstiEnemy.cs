using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ColorShifter), typeof(Collider2D))]
public class ConstiEnemy : MonoBehaviour {
    [SerializeField]
    private Slider deadSlider;
    [SerializeField]
    private float moveInterval;
    [SerializeField]
    private float moveIntervalChange;

    private ColorShifter colorShifter;
    private bool localEaten = false;
    private bool dead = false;
    private float deadTime;

    private bool runsAI;
    private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    private float timeSinceLastMove;
    private Vector2 moveFrom;
    private Vector2 moveTo;

    protected void Start() {
        colorShifter = GetComponent<ColorShifter>();
        colorShifter.enabled = false;
        moveFrom = transform.localPosition;
        moveTo = transform.localPosition;
    }

    public void StartBeingChased() {
        colorShifter.enabled = true;
    }

    public void StopBeingChased() {
        colorShifter.enabled = false;
    }

    public void OnEaten() {
        dead = true;
        deadSlider.value = 1f;
        transform.localScale = Vector3.one * 0.3f;
        deadSlider.gameObject.SetActive(true);
    }

    public bool IsDead() {
        return dead;
    }

    protected void Update() {
        if (dead) {
            deadTime += Time.deltaTime;
            deadSlider.value = deadTime / ConstiServerMiniGame.ChasingDuration;
            if (deadTime >= ConstiServerMiniGame.ChasingDuration) {
                deadTime = 0f;
                dead = false;
                localEaten = false;
                transform.localScale = Vector3.one;
                deadSlider.gameObject.SetActive(false);
                timeSinceLastMove = 0f;
            }
        } else if (runsAI) {
            timeSinceLastMove += Time.deltaTime;
            timeSinceLastMove += Time.deltaTime;
            bool canMove = (timeSinceLastMove >= moveInterval);
            if (canMove) {
                Move();
                timeSinceLastMove = 0f;
                moveInterval -= moveIntervalChange;
            }
            float moveT = Mathf.Clamp01(timeSinceLastMove / moveInterval);
            transform.localPosition = Vector2.Lerp(moveFrom, moveTo, moveCurve.Evaluate(moveT));
        }
    }

    private int lastDirectionIndex = -1;
    private readonly Vector2[] directions = new[] { Vector2.up, Vector2.right, Vector2.down, Vector2.left };
    private void Move() {
        int[] directionIndices = ConstiServerMiniGame.Shuffle(new[] { 0, 1, 2, 3 });
        for (int index = 0; index < directionIndices.Length; index++) {
            var directionIndex = directionIndices[index];
            var direction = directions[directionIndex];

            int oppositeDirectionIndex = (directionIndex + 2) % 4;
            if (lastDirectionIndex == oppositeDirectionIndex) {
                continue;
            }

            if (TryMoveTowards(direction)) {
                lastDirectionIndex = directionIndex;
                return;
            }
        }
        moveFrom = moveTo;
        lastDirectionIndex = -1;
    }

    private bool TryMoveTowards(Vector2 direction) {
        bool isOccupied = Physics2D.OverlapPoint((Vector2)transform.position + direction, LayerMask.GetMask("Ground"));
        bool moveTowards = !isOccupied;
        if (moveTowards) {
            moveFrom = transform.localPosition;
            moveTo = moveFrom + direction;
            moveTo.x = Mathf.Round(moveTo.x);
            moveTo.y = Mathf.Round(moveTo.y + 0.5f) - 0.5f;
        }
        return moveTowards;
    }

    public bool IsLocalEaten() {
        return localEaten;
    }

    public void MarkLocalEaten() {
        localEaten = true;
    }

    public void RunAI() {
        runsAI = true;
    }
}
