using UnityEngine;

public class BussenControllableCharacter : BussenCharacter {
    private BussenClientMiniGame bussenClientMiniGame;
    private int minLaneIndex = 0;
    private int maxLaneIndex = 0;
    private int laneIndex = 0;
    private BussenLane currentLane;

    private bool CanMove(Vector2 direction) {
        return !Physics2D.Raycast(transform.position, direction, 1.4f, LayerMask.GetMask("Ground"));
    }

    private void OnLaneSwitched() {
        currentLane = bussenClientMiniGame.GetLaneAt(laneIndex);
        float x = transform.localPosition.x;
        if (!(currentLane is BussenWaterLane)) {
            x = Mathf.Round(transform.localPosition.x);
        }
        transform.localPosition = new Vector3(
            x,
            transform.localPosition.y,
            transform.localPosition.z
        );
    }

    protected override void Update() {
        base.Update();

        if (currentLane is BussenWaterLane waterLane) {
            transform.localPosition += Vector3.right * (Time.deltaTime * waterLane.GetSpeed());
        }

        if (isAlive) {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) {
                MoveToNextLane();
            } else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) {
                MoveToPreviousLane();
            } else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) {
                MoveHorizontalWithinLane(1);
            } else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) {
                MoveHorizontalWithinLane(-1);
            }
        }
    }

    private void MoveToPreviousLane() {
        if (laneIndex - 1 < minLaneIndex) {
            return;
        }

        if (CanMove(Vector3.down)) {
            transform.localPosition += Vector3.down;
            laneIndex--;
            OnLaneSwitched();
        }
    }

    private void MoveHorizontalWithinLane(int dir) {
        var direction = Vector3.right * dir;
        if (CanMove(direction)) {
            transform.localPosition += direction;
        }
    }

    private void MoveToNextLane() {
        if (laneIndex + 1 >= maxLaneIndex) {
            return;
        }

        if (CanMove(Vector3.up)) {
            transform.localPosition += Vector3.up;
            laneIndex++;
            OnLaneSwitched();
        }
    }

    public bool IsAlive() {
        return isAlive;
    }

    public void SetBussenClientMiniGame(BussenClientMiniGame bussenClientMiniGame) {
        this.bussenClientMiniGame = bussenClientMiniGame;
    }

    public void SetMinLaneIndex(int minLaneIndex) {
        this.minLaneIndex = minLaneIndex;
    }

    public void SetMaxLaneIndex(int maxLaneIndex) {
        this.maxLaneIndex = maxLaneIndex;
    }

    public int GetLaneIndex() {
        return laneIndex;
    }

    protected void OnTriggerEnter2D(Collider2D collision) {
        Kill();
    }
}
