using UnityEngine;

public class BussenMeteor : MonoBehaviour {

    private float pingPongWidth;
    private float speed;

    protected void Update() {
        var current = transform.localPosition;
        var x = current.x + (speed * Time.deltaTime);
        if (x > pingPongWidth / 2) {
            speed *= -1;
            var over = x - (pingPongWidth / 2);
            x -= over;
        }
        if (x < -pingPongWidth / 2) {
            speed *= -1;
            var over = x - (-pingPongWidth / 2);
            x -= over;
        }
        transform.localPosition = new Vector3(x, current.y, current.z);
    }

    public void Initialize(float xPosition, float speed, float pingPongWidth) {
        var current = transform.position;
        current.x = xPosition;
        transform.position = current;

        this.speed = speed;
        this.pingPongWidth = pingPongWidth;
    }
}
