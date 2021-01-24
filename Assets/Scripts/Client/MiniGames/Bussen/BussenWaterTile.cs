using UnityEngine;

public class BussenWaterTile : MonoBehaviour {
    [SerializeField]
    private GameObject lillypad;
    [SerializeField]
    private GameObject duck;

    private float repeatWidth;
    private float speed;

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

    public void Initialize(float speed, float repeatWidth, bool asDuck) {
        this.speed = speed;
        this.repeatWidth = repeatWidth;
        lillypad.SetActive(!asDuck);
        duck.SetActive(asDuck);
    }
}
