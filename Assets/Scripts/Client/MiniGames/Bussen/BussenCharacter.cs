using UnityEngine;

public class BussenCharacter : MonoBehaviour {

    protected bool isAlive = true;

    protected virtual void Update() {
        if (!isAlive) {
            transform.Rotate(new Vector3(0f, 0f, Time.deltaTime * 90f));
        }
    }

    public void Kill() {
        isAlive = false;
    }
}
