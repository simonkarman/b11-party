using UnityEngine;

public class BussenCar : MonoBehaviour {
    private float repeatWidth;
    private float speed;

    public void SetSpeed(float speed) {
        this.speed = speed;
        foreach (var sr in GetComponentsInChildren<SpriteRenderer>(true)) {
            sr.flipX = speed < 0;
        }
    }

    public void SetRepeatWidth(float repeatWidth) {
        this.repeatWidth = repeatWidth;
    }

    protected void Update() {
        var current = transform.localPosition;
        var x = current.x + (speed * Time.deltaTime);
        if (x > repeatWidth / 2) {
            x -= repeatWidth;
        }
        if (x < -repeatWidth / 2) {
            x += repeatWidth;
        }
        transform.localPosition = new Vector3(x, current.y, current.z);
    }

}
