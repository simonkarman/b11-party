using UnityEngine;

public class KamerdienstLocation : MonoBehaviour {
    [SerializeField]
    private Transform start;
    [SerializeField]
    private Transform end;

    public Transform GetStart() {
        return start;
    }

    public Transform GetEnd() {
        return end;
    }
}
