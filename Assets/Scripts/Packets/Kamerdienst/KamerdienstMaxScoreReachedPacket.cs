using Networking;

public class KamerdienstMaxScoreReachedPacket : Packet {
    public KamerdienstMaxScoreReachedPacket(byte[] bytes) : base(bytes) {}
    public KamerdienstMaxScoreReachedPacket() : base(Bytes.Empty) {}

    public override void Validate() { }
}
