using Networking;

public class BussenLastLaneUpdatedPacket : Packet {

    private readonly int lastLaneIndex;

    public BussenLastLaneUpdatedPacket(byte[] bytes) : base(bytes) {
        lastLaneIndex = ReadInt();
    }

    public BussenLastLaneUpdatedPacket(int lastLaneIndex) : base(Bytes.Of(lastLaneIndex)) {
        this.lastLaneIndex = lastLaneIndex;
    }

    public override void Validate() { }

    public int GetLastLaneIndex() {
        return lastLaneIndex;
    }
}
