using Networking;

public class BussenLaneSpawnedPacket : Packet {
    public enum LaneType: int {
        Grass = 0,
        Road = 1,
        Water = 2,
        Lava = 3,
    }

    private readonly int index; // the index of the lane
    private readonly LaneType type; // grass/road/water
    private readonly int seed; // the seed to use for random generation of the trees/cars/logs
    private readonly int amount; // number of trees/cars/logs
    private readonly float multiplier; // mutliplier for: amount of trees, speed of cars, size of logs

    public BussenLaneSpawnedPacket(byte[] bytes) : base(bytes) {
        index = ReadInt();
        type = (LaneType)ReadInt();
        seed = ReadInt();
        amount = ReadInt();
        multiplier = ReadFloat();
    }

    public BussenLaneSpawnedPacket(int index, LaneType type, int seed, int amount, float multiplier) : base(
        Bytes.Pack(Bytes.Of(index), Bytes.Of((int)type), Bytes.Of(seed), Bytes.Of(amount), Bytes.Of(multiplier))
    ) {
        this.type = type;
        this.index = index;
        this.seed = seed;
        this.amount = amount;
        this.multiplier = multiplier;
    }

    public override void Validate() { }

    public int GetIndex() {
        return index;
    }

    public LaneType GetLaneType() {
        return type;
    }

    public int GetSeed() {
        return seed;
    }

    public int GetAmount() {
        return amount;
    }

    public float GetMultiplier() {
        return multiplier;
    }
}
