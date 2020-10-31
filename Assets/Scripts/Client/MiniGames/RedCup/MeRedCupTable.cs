using System;
using UnityEngine;

public class MeRedCupTable : RedCupTable {
    [SerializeField]
    private float directionChangeSpeed = 3f;
    [SerializeField]
    private Transform directionPointer = default;
    [SerializeField]
    private float speedChangeSpeed = 0.5f;
    [SerializeField]
    private float minSpeed = 0.8f;
    [SerializeField]
    private float maxSpeed = 2.3f;
    [SerializeField]
    private float speedMultiplier = 2f;

    public enum RedCupTablePhase {
        WAITING,
        DIRECTION,
        SPEED,
        SHOOTING,
        DONE
    }

    public Action<int> OnHitCup;
    private float currentDirection = 0f;
    private float currentSpeed = 0f;
    private float directionMin = Mathf.PI / 4;
    private float directionMax = Mathf.PI / 2 + Mathf.PI / 4;
    private RedCupTablePhase phase;

    public override void SetFrom(B11PartyClient.B11Client client) {
        base.SetFrom(client);
        for (int cupId = 0; cupId < cups.Length; cupId++) {
            RedCupCup cup = cups[cupId].GetComponent<RedCupCup>();
            cup.SetId(cupId);
        }
        ball.GetComponent<RedCupBall>().OnDone += OnBallDone;
    }

    public void BeginPlaying() {
        phase = RedCupTablePhase.DIRECTION;
    }

    public Vector2 GetBallPosition() {
        return ball.transform.position;
    }

    public void EndPlaying() {
        ball.gameObject.SetActive(false);
        phase = RedCupTablePhase.DONE;
    }

    protected void Update() {
        switch (phase) {
        case RedCupTablePhase.DIRECTION:
            currentDirection += Time.deltaTime * directionChangeSpeed;
            directionPointer.position = ballSpawnPoint.position + GetDirection();
            if (Input.GetKeyDown(KeyCode.Space)) {
                phase = RedCupTablePhase.SPEED;
            }
            break;
        case RedCupTablePhase.SPEED:
            currentSpeed += Time.deltaTime * speedChangeSpeed;
            float speed = minSpeed + Mathf.PingPong(currentSpeed, maxSpeed - minSpeed);
            directionPointer.position = ballSpawnPoint.position + GetDirection() * speed;
            if (Input.GetKeyDown(KeyCode.Space)) {
                phase = RedCupTablePhase.SHOOTING;
                ball.GetComponent<RedCupBall>().Shoot(speed * speedMultiplier, GetDirection());
                currentSpeed = 0f;
                currentDirection = 0f;
            }
            break;
        }
    }

    private void OnBallDone(int cupId) {
        if (cupId >= 0) {
            OnHitCup(cupId);
        }
        ball.transform.position = ballSpawnPoint.position;
        phase = RedCupTablePhase.DIRECTION;
    }

    private Vector3 GetDirection() {
        float range = directionMax - directionMin;
        float directionAngle = directionMin + Mathf.PingPong(currentDirection, range);
        return new Vector3(
            Mathf.Cos(directionAngle),
            Mathf.Sin(directionAngle),
            0f
        );

    }
}