using System;
using UnityEngine;

public class BussenCharacter : MonoBehaviour {

    private bool isAlive = true;
    private int minLaneIndex = 0;
    private int maxLaneIndex = 0;
    private int laneIndex = 0;

    protected void Update() {
        if (!isAlive) {
            return;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) {
            MoveToNextLane();
        } else if(Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) {
            MoveToPreviousLane();
        } else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) {
            MoveOnLane(1);
        } else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) {
            MoveOnLane(-1);
        }
    }

    private void MoveToPreviousLane() {
        if (laneIndex - 1 < minLaneIndex) {
            return;
        }

        // TODO: only if no collision with tree
        transform.localPosition += Vector3.down;
        laneIndex--;
    }

    private void MoveOnLane(int direction) {
        // TODO only if no collision
        transform.localPosition += Vector3.right * direction;
    }

    private void MoveToNextLane() {
        if (laneIndex + 1 >= maxLaneIndex) {
            return;
        }

        // TODO only if no collision with tree
        transform.localPosition += Vector3.up;
        laneIndex++;
    }

    public bool IsAlive() {
        return isAlive;
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

    public void Kill() {
        isAlive = false;
    }

    protected void OnCollisionEnter2D(Collision2D collision) {
        /*isAlive = false;
        rb2d.constraints = RigidbodyConstraints2D.FreezeAll;
        transform.localPosition = Vector3.down * 20;*/
    }
}
