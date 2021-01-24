using System.Linq;
using UnityEngine;

public class BussenWaterLane : BussenLane {
    private float speed;

    [SerializeField]
    private BussenWaterTile[] tiles;

    public override void SetFrom(int seed, int amount, float multiplier) {
        var random = new System.Random(seed);
        bool flowDirection = random.Next(0, 2) == 0;
        speed = multiplier * (flowDirection ? -1 : 1);

        // TODO: replace <amount> lillypad(s) with a duck
        int[] randomPositions = Shuffle(AllLinePositions(), random).Take(amount).ToArray();
        for (int i = 0; i < tiles.Length; i++) {
            var tile = tiles[i];
            tile.Initialize(speed, LaneWidth + 2, randomPositions.Contains(i));
        }
    }

    public float GetSpeed() {
        return speed;
    }
}
