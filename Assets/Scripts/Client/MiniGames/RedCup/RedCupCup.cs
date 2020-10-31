using UnityEngine;

public class RedCupCup : MonoBehaviour {
    private int cupId;

    public void SetId(int cupId) {
        this.cupId = cupId;
    }

    public int GetCupId() {
        return cupId;
    }
}