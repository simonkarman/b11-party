using Networking;

public class KetelVangenSpawnedPacket : Packet {
    public enum SpawnType: int {
        KETEL_1 = 0,
        KETEL_1_MATUUR = 1,
        SMIRNOFF_ICE = 2,
    }

    private readonly SpawnType spawnType;
    private readonly float xPositionT;
    private readonly float speed;

    public KetelVangenSpawnedPacket(byte[] bytes) : base(bytes) {
        spawnType = (SpawnType)ReadInt();
        xPositionT = ReadFloat();
        speed = ReadFloat();
    }

    public KetelVangenSpawnedPacket(SpawnType spawnType, float xPositionT, float speed) : base(
        Bytes.Pack(Bytes.Of((int)spawnType), Bytes.Of(xPositionT), Bytes.Of(speed))
    ) {
        this.spawnType = spawnType;
        this.xPositionT = xPositionT;
        this.speed = speed;
    }

    public override void Validate() { }

    public SpawnType GetSpawnType() {
        return spawnType;
    }

    public float GetXPositionT() {
        return xPositionT;
    }

    public float GetSpeed() {
        return speed;
    }
}
