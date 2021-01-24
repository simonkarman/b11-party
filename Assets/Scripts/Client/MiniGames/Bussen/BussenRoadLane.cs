using System.Linq;
using UnityEngine;

public class BussenRoadLane : BussenLane {
    [SerializeField]
    private GameObject carPrefab;

    public override void SetFrom(int seed, int amount, float multiplier) {
        var random = new System.Random(seed);
        bool drivingDirection = random.Next(0, 2) == 0;
        float speed = multiplier * (drivingDirection ? -1 : 1);

        int[] randomPositions = Shuffle(AllLinePositions(), random).Take(amount).ToArray();
        for (int i = 0; i < randomPositions.Length; i++) {
            int randomPosition = randomPositions[i];
            Transform instance = Instantiate(carPrefab, content).transform;
            instance.name = $"{LaneIndex}.{i}: {carPrefab.name} at {randomPosition}";
            instance.localPosition = Vector3.right * randomPosition * 2f;
            BussenCar car = instance.GetComponent<BussenCar>();
            car.SetSpeed(speed);
            car.SetRepeatWidth(LaneWidth * 2f);
        }
    }
}
