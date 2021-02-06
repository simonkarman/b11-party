using UnityEngine;

public class ResetGlobalRotationOnStart : MonoBehaviour {
    protected void Start() {
        transform.rotation = Quaternion.identity;
    }
}
