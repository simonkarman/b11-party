using Networking;

public class BatavierenObstacleSpawnedPacket : Packet {
    public enum Mode: int {
        GROUND = 0,
        AIR = 1,
    }

    private readonly Mode mode;
    private readonly float speed;

    public BatavierenObstacleSpawnedPacket(byte[] bytes) : base(bytes) {
        mode = (Mode)ReadInt();
        speed = ReadFloat();
    }

    public BatavierenObstacleSpawnedPacket(Mode mode, float speed) : base(
        Bytes.Pack(Bytes.Of((int)mode), Bytes.Of(speed))
    ) {
        this.mode = mode;
        this.speed = speed;
    }

    public override void Validate() { }

    public Mode GetMode() {
        return mode;
    }

    public float GetSpeed() {
        return speed;
    }
}
