using UnityEngine;

public class ConstiMap : MonoBehaviour {
    [SerializeField]
    private Transform root;
    [SerializeField]
    private Transform spawns;
    [SerializeField]
    private Transform blocks;
    [SerializeField]
    private Transform coins;
    [SerializeField]
    private Transform powerups;
    [SerializeField]
    private Transform enemies;

    public Transform GetRoot() {
        return root;
    }

    public Transform GetSpawns() {
        return spawns;
    }

    public Transform GetBlocks() {
        return blocks;
    }

    public Transform GetCoins() {
        return coins;
    }

    public Transform GetPowerups() {
        return powerups;
    }

    public Transform GetEnemies() {
        return enemies;
    }
}
