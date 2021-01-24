using UnityEngine;

public class BussenLavaLane : BussenLane {
    [SerializeField]
    private BussenMeteor meteor;

    public override void SetFrom(int seed, int amount, float multiplier) {
        var random = new System.Random(seed);
        bool flowDirection = random.Next(0, 2) == 0;
        float speed = multiplier * (flowDirection ? -1 : 1) * 2;

        float xPosition = ((float)random.NextDouble() - 0.5f) * LaneWidth;
        meteor.Initialize(xPosition, speed, LaneWidth);
    }
}
