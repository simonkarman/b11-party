using System.Linq;
using UnityEngine;

public class BussenGrassLane : BussenLane {
    [SerializeField]
    private GameObject grassObstaclePrefab;

    public override void SetFrom(int seed, int amount, float _) {
        var random = new System.Random(seed);
        int[] randomPositions = Shuffle(LaneIndex > 4 ? AllLinePositions() : NoneCenterLinePositions(), random).Take(amount).ToArray();
        for (int i = 0; i < randomPositions.Length; i++) {
            int randomPosition = randomPositions[i];
            Transform instance = Instantiate(grassObstaclePrefab, content).transform;
            instance.name = $"{LaneIndex}.{i}: {grassObstaclePrefab.name} at {randomPosition}";
            instance.localPosition = Vector3.right * randomPosition;
        }
    }
}
